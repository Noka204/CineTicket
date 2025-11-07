
/* ========= Config ========= */
const BASE_URL = "https://localhost:7058";
const API = {
  phim: (id, lang) => `${BASE_URL}/api/phim/get/${id}?lang=${encodeURIComponent(lang||'vi')}`,
  cities: `${BASE_URL}/api/rap/get-cities`,
  rapsByCity: (city) => `${BASE_URL}/api/rap/by-city?thanhPho=${encodeURIComponent(city||"")}`,
  suatByPhim: (phimId, params) => {
    const p = new URLSearchParams(params || {});
    return `${BASE_URL}/api/suatchieu/get-by-phim/${phimId}?${p.toString()}`;
  }
};
const $  = (s,r=document)=>r.querySelector(s);
const $$ = (s,r=document)=>Array.from(r.querySelectorAll(s));
const authHeader = localStorage.getItem("token") ? { Authorization: `Bearer ${localStorage.getItem("token")}` } : {};
// Chuẩn hoá ảnh giống trang Admin (bỏ wwwroot/, nối BASE_URL nếu là relative)
const absUrl = (u)=>{
  if (!u) return "";
  if (/^https?:\/\//i.test(u)) return u;
  const clean = String(u).replace(/^\/?wwwroot\//i, "");
  return `${BASE_URL.replace(/\/+$/,"")}/${clean.replace(/^\/+/,"")}`;
};

/* ========= I18N ========= */
const CULT = { vi: "vi", en: "en", fr: "fr" };  // alias ngắn
let curAlias = localStorage.getItem("lang") || "vi";
const I18N_CACHE = {};
async function i18nLoad(alias){
  const a = CULT[alias] ? alias : "vi";
  if (I18N_CACHE[a]) return I18N_CACHE[a];
  try{
    const url = new URL(`${BASE_URL}/api/i18n`);
    url.searchParams.set("lang", a);
    // chống cache
    url.searchParams.set("_", Date.now());
    const res = await fetch(url, { credentials: "include", cache: "no-store" });
    if (!res.ok) throw 0;
    const dict = await res.json();
    I18N_CACHE[a] = (dict && typeof dict === "object" && !Array.isArray(dict)) ? dict : {};
  }catch{
    I18N_CACHE[a] = {};
  }
  return I18N_CACHE[a];
}
function t(key){
  const pack = I18N_CACHE[curAlias] || {};
  const v = pack[key];
  return (typeof v === "string") ? v : null;
}
function tr(key, fallback){ const v = t(key); return v == null ? fallback : v; }
function i18nFmt(key, ...args){
  let s = tr(key, key);
  args.forEach((v,i)=> s = s.replace(new RegExp(`\\{${i}\\}`,'g'), v));
  return s;
}
function applyI18nToDom(root=document){
  root.querySelectorAll("[data-i18n]").forEach(el=>{
    const k = el.getAttribute("data-i18n");
    const v = t(k);
    if (v!=null) el.textContent = v;
  });
  const titleEl = document.querySelector("title[data-i18n]");
  if (titleEl){
    const tv = t(titleEl.getAttribute("data-i18n"));
    if (tv!=null) document.title = tv;
  }
  document.documentElement.lang = curAlias;
}
// ĐỔI NGÔN NGỮ: lưu, ping BE cookie, áp i18n, cập nhật URL ?lang=..., refetch chi tiết ngay (hủy request cũ)
async function setLang(alias){
  curAlias = CULT[alias] ? alias : "vi";
  localStorage.setItem("lang", curAlias);
  try{ await fetch(`${BASE_URL}/api/ping?lang=${encodeURIComponent(curAlias)}&t=${Date.now()}`, { credentials: "include", cache: "no-store" }); }catch{}
  await i18nLoad(curAlias);
  applyI18nToDom(document);
  const u = new URL(location.href);
  u.searchParams.set("lang", curAlias);
  history.replaceState({}, "", u.toString());
  try{ renderMovieDetail(); }catch{}
}
document.addEventListener("click",(e)=>{
  const langLink = e.target.closest("[data-set-lang]");
  if (!langLink) return;
  e.preventDefault();
  setLang(langLink.getAttribute("data-set-lang"));
});

/* ========= Scroll lock ========= */
function lockBodyScroll(){ document.documentElement.style.overflow='hidden'; document.body.style.overflow='hidden'; }
function unlockBodyScroll(){ document.documentElement.style.overflow=''; document.body.style.overflow=''; }

/* ========= Header/Footer ========= */
async function includeHTML(id, file){
  try{
    const html = await fetch(`${file}?t=${Date.now()}`, { cache:"no-store" }).then(r=>r.text());
    const host = document.getElementById(id);
    if (host) host.innerHTML = html;
    applyI18nToDom(host || document);
    if (id==="include-header" && typeof updateAuthButtons==="function"){
      setTimeout(updateAuthButtons, 80);
    }
  }catch(e){ console.error(e); }
}

/* ========= Trailer modal ========= */
function toEmbed(link){
  if (!link) return "";
  if (link.includes("watch?v=")) return link.replace("watch?v=","embed/");
  if (link.includes("youtu.be/")) return link.replace("youtu.be/","youtube.com/embed/");
  return link;
}
function ensureTrailerModal(){
  if ($('#trailerModal')) return;
  const ov = document.createElement("div");
  ov.id="trailerModal";
  ov.className="modal-overlay";
  ov.innerHTML = `
    <div class="modal-content video">
      <button class="close-btn" aria-label="${tr('Trailer_Close','Đóng')}">&times;</button>
      <iframe id="trailerFrame" width="100%" height="100%" frameborder="0" allowfullscreen title="${tr('Trailer_Title','Trailer')}"></iframe>
    </div>`;
  document.body.appendChild(ov);
  ov.addEventListener("click", (e)=>{
    if (e.target===ov || e.target.closest(".close-btn")) closeTrailerModal();
  });
}
function openTrailerModal(youtubeLink){
  ensureTrailerModal();
  const embed = toEmbed(youtubeLink);
  if (!embed) return;
  $('#trailerFrame').src = embed;
  $('#trailerModal').style.display = 'flex';
  lockBodyScroll();
}
function closeTrailerModal(){
  const m = $('#trailerModal'); if (!m) return;
  m.style.display = 'none';
  $('#trailerFrame').src = '';
  unlockBodyScroll();
}

/* ========= State ========= */
let MOVIE = null;
let MOVIE_ID = null;
let MOVIE_CTRL = null;     // AbortController hiện hành
let MOVIE_REQ_VER = 0;     // version để chặn “đua” request

const STATE = { day:null, city:null, rapId:null };

/* ========= Utils ========= */
const pad2 = (n)=>String(n).padStart(2,"0");
const fmtYMD = (d)=>`${d.getFullYear()}-${pad2(d.getMonth()+1)}-${pad2(d.getDate())}`;
function normTimeStr(x){
  const raw = x?.gioChieu || x?.thoiGianBatDau || x?.ThoiGianBatDau || '';
  if (!raw) return '';
  if (/^\d{2}:\d{2}$/.test(raw)) return raw;
  const d = new Date(raw);
  if (!isNaN(d)) return `${pad2(d.getHours())}:${pad2(d.getMinutes())}`;
  return raw.slice(0,5);
}
const timeToMinutes = (hhmm)=> {
  const [h,m]=(hhmm||"").split(":").map(v=>parseInt(v,10));
  return (isNaN(h)||isNaN(m))?0:h*60+m;
};

/* ========= Render chi tiết (có hủy request + chống cache + chặn đua) ========= */
async function renderMovieDetail(){
  const params = new URLSearchParams(location.search);
  MOVIE_ID = Number(params.get("id"));
  if (!MOVIE_ID){
    const mount = $("#movieDetail");
    if (mount) mount.innerHTML = `<p style='color:red'>${tr('Detail_NoData','Không tìm thấy phim.')}</p>`;
    return;
  }

  // Hủy request cũ
  if (MOVIE_CTRL) try{ MOVIE_CTRL.abort(); }catch{}
  MOVIE_CTRL = new AbortController();
  const myVer = ++MOVIE_REQ_VER;

  const host = $("#movieDetail");
  if (host) host.innerHTML = `<p style='opacity:.8'>${tr('Loading_Detail','Đang tải chi tiết…')}</p>`;

  try{
    // thêm tham số chống cache
    const url = new URL(API.phim(MOVIE_ID, curAlias));
    url.searchParams.set("_", Date.now().toString());

    const res = await fetch(url.toString(), {
      signal: MOVIE_CTRL.signal,
      cache: "no-store",
      headers: { ...authHeader, "X-No-Cache": Date.now().toString() }
    });
    const j = await res.json();

    // Nếu đã có request mới hơn, bỏ qua
    if (myVer !== MOVIE_REQ_VER) return;

    if (!j?.status || !j?.data) throw new Error("no data");
    MOVIE = j.data;

    const poster = MOVIE.poster ? absUrl(MOVIE.poster) : "../../assets/images/default-poster.jpg";
    const theLoaiText = Array.isArray(MOVIE.loaiPhims) && MOVIE.loaiPhims.length
      ? MOVIE.loaiPhims.map(l=>l.tenLoaiPhim).join(", ")
      : tr("Common_NotAvailable","Chưa cập nhật");

    host.innerHTML = `
      <div class="movie-poster-wrapper">
        <div class="movie-poster" style="background-image:url('${poster}')"></div>
        ${MOVIE.trailer ? `<button class="btn-trailer">▶️ ${tr('Detail_ViewTrailer','Xem trailer')}</button>` : ``}
      </div>
      <div class="movie-info">
        <h1>${MOVIE.tenPhim||''}</h1>
        <p><strong>${tr('Detail_Genres','Thể loại')}:</strong> ${theLoaiText}</p>
        <p><strong>${tr('Detail_Duration','Thời lượng')}:</strong> ${MOVIE.thoiLuong||''} ${tr('Detail_Duration_MinUnit','phút')}</p>
        <p><strong>${tr('Detail_Director','Đạo diễn')}:</strong> ${MOVIE.daoDien||tr('Common_NotAvailable','Chưa cập nhật')}</p>
        <p><strong>${tr('Detail_Language','Ngôn ngữ')}:</strong> ${MOVIE.ngonNgu||tr('Common_NotAvailable','Chưa cập nhật')}</p>
        <p><strong>${tr('Detail_Cast','Diễn viên')}:</strong> ${MOVIE.dienVien||tr('Common_NotAvailable','Chưa cập nhật')}</p>
        <p><strong>${tr('Detail_ReleaseDate','Khởi chiếu')}:</strong> ${MOVIE.khoiChieu||tr('Common_NotAvailable','Chưa cập nhật')}</p>
        <p><strong>${tr('Detail_Synopsis','Mô tả')}:</strong> ${MOVIE.moTa||tr('Common_NoDescription','Không có mô tả.')}</p>
        <a href="../../pages/booking/select-seats.html?phimId=${MOVIE.maPhim}" class="btn-booking">🎟 ${tr('Detail_BookNow','Đặt vé ngay')}</a>

        <div id="trailerContainer" class="trailer-container" style="display:none;">
          <iframe id="trailerFrameInline" width="100%" height="400" frameborder="0" allowfullscreen title="${tr('Trailer_Title','Trailer')}"></iframe>
        </div>
      </div>`;
  }catch(err){
    if (err?.name === "AbortError") return; // bị hủy do đổi ngôn ngữ
    console.error(err);
    if (host) host.innerHTML = `<p style='color:#ffb4b4'>${tr('Error_Detail','Không thể tải chi tiết phim.')}</p>`;
  }
}

/* ========= Booking modal ========= */
function ensureBookModal(){
  if ($('#bookModal')) return;
  const wrap = document.createElement('div');
  wrap.id = 'bookModal';
  wrap.className = 'modal';
  wrap.innerHTML = `
    <div class="modal-content">
      <div class="modal-header">
        <button id="bookClose" class="modal-close" aria-label="${tr('Trailer_Close','Đóng')}">✕</button>
        <div class="form-group" style="display:flex;gap:14px;align-items:center;margin:0">
          <img id="bookPoster" src="" alt="poster" style="width:64px;height:92px;object-fit:cover;border-radius:8px;border:1px solid rgba(255,255,255,.12)">
          <div style="flex:1">
            <div id="bookMovieName" style="font-weight:800;font-size:1.1rem"></div>
            <div id="bookMovieMeta" style="opacity:.8"></div>
          </div>
        </div>
      </div>

      <div class="form-group">
        <label>${tr('Label_ShowDate','Ngày chiếu (29 ngày tới)')}</label>
        <div id="chipsDays" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr('Label_City','Thành phố')}</label>
        <div id="chipsCities" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr('Label_Cinema','Rạp')}</label>
        <div id="chipsRaps" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr('Label_RoomShow','Phòng & Suất')}</label>
        <div id="roomShowtimes"></div>
      </div>
    </div>`;
  document.body.appendChild(wrap);

  const closeBookModal = ()=>{ wrap.style.display='none'; unlockBodyScroll(); };
  $('#bookClose').addEventListener('click', closeBookModal);
  wrap.addEventListener('click', (e)=>{ if (e.target===wrap) closeBookModal(); });
}
function openBookModal(){
  ensureBookModal();
  const poster = MOVIE?.poster ? absUrl(MOVIE.poster) : "../../assets/images/default-poster.jpg";
  $('#bookPoster').src = poster;
  $('#bookMovieName').textContent = MOVIE?.tenPhim || '';
  $('#bookMovieMeta').textContent =
    (MOVIE?.thoiLuong ? `${MOVIE.thoiLuong} ${tr('Detail_Duration_MinUnit','phút')}` : "") +
    (MOVIE?.ngonNgu ? ` • ${MOVIE.ngonNgu}` : "");

  const hostDay = $('#chipsDays'); hostDay.innerHTML = '';
  const today = new Date();
  STATE.day = fmtYMD(today);
  for (let i=0;i<29;i++){
    const d = new Date(today); d.setDate(d.getDate()+i);
    const chip = document.createElement('div');
    chip.className = 'chip' + (i===0 ? ' active' : '');
    chip.dataset.day = fmtYMD(d);
    chip.textContent = `${pad2(d.getDate())}/${pad2(d.getMonth()+1)}`;
    chip.addEventListener('click', ()=>{
      hostDay.querySelectorAll('.chip').forEach(c=>c.classList.remove('active'));
      chip.classList.add('active');
      STATE.day = chip.dataset.day;
      if (STATE.city && STATE.rapId) loadShowtimes();
    });
    hostDay.appendChild(chip);
  }

  loadCities().then(()=>{ if (STATE.city) loadRapsByCity(); });
  $('#bookModal').style.display = 'flex';
  lockBodyScroll();
}
async function loadCities(){
  const host = $('#chipsCities'); host.innerHTML = `<div class="chip">${tr('Loading_Cities','Đang tải thành phố…')}</div>`;
  try{
    const j = await fetch(`${API.cities}?t=${Date.now()}`, { cache:"no-store" }).then(r=>r.json());
    const list = Array.isArray(j?.data) ? j.data : (Array.isArray(j) ? j : []);
    host.innerHTML = '';
    if (!list.length){ host.innerHTML = `<span class="chip">${tr('NoData_Cities','Không có dữ liệu')}</span>`; return; }
    STATE.city = list.find(x=>/hồ chí minh/i.test(x)) || list[0];
    list.forEach(city=>{
      const chip = document.createElement('div');
      chip.className = 'chip' + (city===STATE.city?' active':'');
      chip.textContent = city;
      chip.addEventListener('click', ()=>{
        host.querySelectorAll('.chip').forEach(c=>c.classList.remove('active'));
        chip.classList.add('active');
        STATE.city = city; STATE.rapId = null;
        loadRapsByCity();
      });
      host.appendChild(chip);
    });
  }catch{
    host.innerHTML = `<span class="chip">${tr('Error_Cities','Lỗi tải thành phố')}</span>`;
  }
}
async function loadRapsByCity(){
  const host = $('#chipsRaps'); host.innerHTML = `<div class="chip">${tr('Loading_Cinemas','Đang tải rạp…')}</div>`;
  try{
    const j = await fetch(`${API.rapsByCity(STATE.city)}&t=${Date.now()}`, { cache:"no-store" }).then(r=>r.json());
    const list = Array.isArray(j?.data) ? j.data : (Array.isArray(j) ? j : []);
    host.innerHTML = '';
    if (!list.length){ host.innerHTML = `<span class="chip">${tr('NoData_Cinemas','Không có rạp')}</span>`; return; }
    STATE.rapId = list[0]?.maRap ?? list[0]?.MaRap ?? null;

    list.forEach(rap=>{
      const id = rap.maRap ?? rap.MaRap, name = rap.tenRap ?? rap.TenRap;
      const chip = document.createElement('div');
      chip.className = 'chip' + (id===STATE.rapId?' active':'');
      chip.textContent = name || i18nFmt("Cinema_Number_Tpl", id || "");
      chip.addEventListener('click', ()=>{
        host.querySelectorAll('.chip').forEach(c=>c.classList.remove('active'));
        chip.classList.add('active');
        STATE.rapId = id;
        loadShowtimes();
      });
      host.appendChild(chip);
    });

    loadShowtimes();
  }catch{
    host.innerHTML = `<span class="chip">${tr('NoData_Cinemas','Lỗi tải rạp')}</span>`;
  }
}
async function loadShowtimes(){
  const host = $('#roomShowtimes');
  host.innerHTML = `<div class="no-data">${tr('Loading_Showtimes','Đang tải suất chiếu…')}</div>`;

  if (!MOVIE?.maPhim || !STATE.rapId || !STATE.day){
    host.innerHTML = `<div class="no-data">${tr('Info_MissingFilters','Thiếu thông tin (ngày/thành phố/rạp).')}</div>`;
    return;
  }
  try{
    const url = API.suatByPhim(MOVIE.maPhim, { maRap: STATE.rapId, ngay: STATE.day });
    const res = await fetch(`${url}&t=${Date.now()}`, { cache:"no-store" });
    const json = await res.json();
    const list = Array.isArray(json?.data) ? json.data : (Array.isArray(json) ? json : []);
    if (!list.length){ host.innerHTML = `<div class="no-data">${tr('NoData_Showtimes','Không có suất chiếu.')}</div>`; return; }

    const byRoom = new Map();
    for (const s of list){
      const pid = s.maPhong ?? s.MaPhong ?? s.maPhongId ?? s.MaPhongId ?? null;
      const name = s.tenPhong ?? s.TenPhong ?? i18nFmt("Room_Number_Tpl", pid || "");
      (byRoom.get(pid) ?? byRoom.set(pid, { pid, name, items: [] }).get(pid)).items.push(s);
    }

    host.innerHTML = '';
    for (const {pid, name, items} of byRoom.values()){
      const card = document.createElement('div');
      card.className = 'room-card';
      card.innerHTML = `
        <div class="room-head"><div class="room-name">🪑 ${name}</div></div>
        <div class="time-chips"></div>`;
      const zone = card.querySelector('.time-chips');

      items.sort((a,b)=> timeToMinutes(normTimeStr(a)) - timeToMinutes(normTimeStr(b)));
      items.forEach(s=>{
        const suatId = s.maSuat ?? s.MaSuat ?? s.id ?? s.Id;
        const tlabel = normTimeStr(s) || '—';
        const btn = document.createElement('button');
        btn.className = 'time-chip';
        btn.textContent = tlabel;
        btn.addEventListener('click', ()=>{
          const params = new URLSearchParams({
            phimId: MOVIE.maPhim,
            suatId,
            ngay: STATE.day,
            rapId: STATE.rapId,
            roomId: pid
          });
          location.href = `../../pages/booking/select-seats.html?${params.toString()}`;
        });
        zone.appendChild(btn);
      });

      host.appendChild(card);
    }
  }catch(e){
    console.error(e);
    host.innerHTML = `<div class="no-data">${tr('Error_Showtimes','Lỗi tải suất chiếu.')}</div>`;
  }
}

/* ========= Delegation ========= */
document.addEventListener("click", (e)=>{
  const tbtn = e.target.closest(".btn-trailer");
  if (tbtn){
    e.preventDefault();
    if (!MOVIE?.trailer){
      const frame = $('#trailerFrameInline'), box = $('#trailerContainer');
      if (frame && box){ box.style.display='block'; frame.src=''; }
      return;
    }
    openTrailerModal(MOVIE.trailer);
  }
  const bbtn = e.target.closest(".btn-booking");
  if (bbtn){
    e.preventDefault();
    openBookModal();
  }
});

/* ========= MoMo callback ========= */
(function runMomoCallback() {
  try {
    const q = new URLSearchParams(location.search);
    if (!q.get('orderId')) return;
    const payload = {
      partnerCode:  q.get('partnerCode')  || '',
      orderId:      q.get('orderId')      || '',
      requestId:    q.get('requestId')    || '',
      amount:       q.get('amount')       || '',
      orderInfo:    q.get('orderInfo')    || '',
      orderType:    q.get('orderType')    || '',
      transId:      q.get('transId')      || '',
      resultCode:   q.get('resultCode')   || '',
      message:      q.get('message')      || '',
      payType:      q.get('payType')      || '',
      responseTime: q.get('responseTime') || '',
      extraData:    q.get('extraData')    || '',
      signature:    q.get('signature')    || ''
    };
    fetch(`${BASE_URL}/api/momopayment/confirm`, {
      method:'POST',
      headers:{'Content-Type':'application/json'},
      body:JSON.stringify(payload)
    }).catch(()=>{}).finally(()=>{ history.replaceState({}, "", location.pathname + location.hash); });
  } catch {
    try{ history.replaceState({}, "", location.pathname + location.hash); }catch{}
  }
})();

/* ========= Boot ========= */
document.addEventListener("DOMContentLoaded", async ()=>{
  // đọc lang từ URL nếu có
  const langQ = new URLSearchParams(location.search).get("lang");
  if (CULT[langQ]) { curAlias = langQ; localStorage.setItem("lang", curAlias); }

  await i18nLoad(curAlias);               // nạp từ điển trước
  applyI18nToDom(document);
  await includeHTML("include-header", "../../part/header.html");
  await includeHTML("include-footer", "../../part/footer.html");
  renderMovieDetail();                    // refetch chi tiết (có AbortController)
});

