
const BASE_URL = "https://localhost:7058";
const API = {
  phimAll: `${BASE_URL}/api/phim/get-all`,
  phimById: (id, lang) => `${BASE_URL}/api/phim/get/${id}?lang=${encodeURIComponent(lang||'vi')}`,
  cities: `${BASE_URL}/api/rap/get-cities`,
  rapsByCity: (city) => `${BASE_URL}/api/rap/by-city?thanhPho=${encodeURIComponent(city||"")}`,
  suatByPhim: (phimId, params) => `${BASE_URL}/api/suatchieu/get-by-phim/${phimId}?${new URLSearchParams(params||{}).toString()}`
};
const $  = (s, r=document)=>r.querySelector(s);
const $$ = (s, r=document)=>Array.from(r.querySelectorAll(s));

/* ẢNH: chuẩn hoá giống trang Admin */
function absUrl(u) {
  if (!u) return "";
  if (/^https?:\/\//i.test(u)) return u;
  const clean = String(u).replace(/^\/?wwwroot\//i, "");
  return `${BASE_URL.replace(/\/+$/, "")}/${clean.replace(/^\/+/, "")}`;
}
const escapeHtml = (s)=>(s??"").toString().replace(/[&<>"']/g,m=>({"&":"&amp;","<":"&lt;",">":"&gt;","\"":"&quot;","'":"&#39;"}[m]));

/* ================== I18N ================== */
const CULT = { vi:"vi-VN", en:"en-US", fr:"fr-FR" };
let curAlias = localStorage.getItem("lang") || "vi";
const I18N_CACHE = {};
async function i18nLoad(alias){
  const a = CULT[alias] ? alias : "vi";
  if (I18N_CACHE[a]) return I18N_CACHE[a];
  try{
    const url = new URL(`${BASE_URL}/api/i18n`);
    url.searchParams.set("lang", a);
    url.searchParams.set("_", Date.now()); // chống cache
    const res = await fetch(url, { credentials:"include", cache:"no-store" });
    if (!res.ok) throw 0;
    const dict = await res.json();
    I18N_CACHE[a] = (dict && typeof dict === "object" && !Array.isArray(dict)) ? dict : {};
  }catch{ I18N_CACHE[a] = {}; }
  return I18N_CACHE[a];
}
function t(key){
  const pack = I18N_CACHE[curAlias] || {};
  const v = pack[key];
  return typeof v === "string" ? v : null;
}
function tr(key, fallback){ const v = t(key); return v==null?fallback:v; }
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
  document.querySelectorAll("[data-set-lang]").forEach(a=>{
    a.classList.toggle("active", a.getAttribute("data-set-lang")===curAlias);
  });
}

/* ================== RACE-SAFE FETCHERS ================== */
/* Mẫu factory trả về fetch “latest-wins” (luôn hủy request trước đó) */
function makeLatestFetcher(){
  let ctl = null;
  return async (input, init={})=>{
    if (ctl) ctl.abort();
    ctl = new AbortController();
    const url = typeof input === 'string' ? input : input.toString();
    // ép no-store + timestamp chống cache
    const u = new URL(url, location.origin);
    if (!u.searchParams.has('_')) u.searchParams.set('_', Date.now());
    const res = await fetch(u, { ...init, signal: ctl.signal, cache:'no-store' });
    return res;
  };
}

/* fetchers riêng cho từng luồng để cô lập đường đua */
const fetchMoviesLatest     = makeLatestFetcher();
const fetchBannerLatest     = makeLatestFetcher();
const fetchDetailLatest     = makeLatestFetcher();
const fetchCitiesLatest     = makeLatestFetcher();
const fetchRapsLatest       = makeLatestFetcher();
const fetchShowtimesLatest  = makeLatestFetcher();

/* ================== LANG SWITCH ================== */
async function setLang(alias){
  curAlias = CULT[alias] ? alias : "vi";
  localStorage.setItem("lang", curAlias);
  // ping để set cookie văn hoá nếu server dùng
  fetch(`${BASE_URL}/api/ping?lang=${encodeURIComponent(curAlias)}&_=${Date.now()}`, {credentials:"include", cache:'no-store'}).catch(()=>{});
  await i18nLoad(curAlias);
  applyI18nToDom(document);

  // Refetch ngay các khối đang hiển thị (latest-wins)
  try{ loadMovies(true); }catch{}
  try{ loadBannerSlideshow(true); }catch{}
  try{
    const detailMount = document.getElementById('movieDetail');
    if (detailMount) loadMovieDetailInto(detailMount, true);
  }catch{}
}

document.addEventListener("click",(e)=>{
  const langLink = e.target.closest("[data-set-lang]");
  if (!langLink) return;
  e.preventDefault();
  setLang(langLink.getAttribute("data-set-lang"));
});

/* ================== UTILS ================== */
function getQuery(k){ return new URLSearchParams(location.search).get(k); }

/* ================== SCROLL LOCK (Modal) ================== */
function lockBodyScroll(){ document.documentElement.style.overflow='hidden'; document.body.style.overflow='hidden'; }
function unlockBodyScroll(){ document.documentElement.style.overflow=''; document.body.style.overflow=''; }

/* ================== INCLUDE HEADER/FOOTER ================== */
async function includeHTML(id, file){
  try{
    const html = await fetch(file, {cache:'no-store'}).then(r=>r.text());
    const el = document.getElementById(id);
    if (el) el.innerHTML = html;
    if (id==="include-header"){
      requestAnimationFrame(()=>{
        applyI18nToDom(el||document);
        try{ updateAuthButtons && updateAuthButtons(); }catch{}
      });
    } else {
      applyI18nToDom(el||document);
    }
  }catch(e){ console.error(e); }
}

/* ================== TRAILER MODAL ================== */
function toEmbed(link){
  if (!link) return "";
  if (link.includes("watch?v=")) return link.replace("watch?v=","embed/");
  if (link.includes("youtu.be/")) return link.replace("youtu.be/","youtube.com/embed/");
  return link;
}
function openTrailerModal(youtubeLink){
  const embed = toEmbed(youtubeLink);
  if (!embed) return;
  const f = $('#trailerFrame');
  const m = $('#trailerModal');
  if (!f || !m) return;
  f.src = embed;
  m.style.display = 'flex';
  lockBodyScroll();
}
function closeTrailerModal(){
  const f = $('#trailerFrame');
  const m = $('#trailerModal');
  if (f) f.src='';
  if (m) m.style.display='none';
  unlockBodyScroll();
}
window.openTrailerModal = openTrailerModal;
window.closeTrailerModal = closeTrailerModal;

/* ================== BANNER ================== */
let bannerSlideIndex = 0, bannerTimer=null;
function showBannerSlide(i){
  const slides = $$(".banner-slide");
  const dots = $$(".banner-dot");
  slides.forEach((s,idx)=>s.classList.toggle("active", idx===i));
  dots.forEach((d,idx)=>d.classList.toggle("active", idx===i));
  bannerSlideIndex = i;
}
function changeBannerSlide(n){
  const slides = $$(".banner-slide"); if (!slides.length) return;
  bannerSlideIndex = (bannerSlideIndex + n + slides.length) % slides.length;
  showBannerSlide(bannerSlideIndex);
}
window.changeBannerSlide = changeBannerSlide;
window.currentSlide = function(){};

function startBannerAutoSlide(){
  clearInterval(bannerTimer);
  bannerTimer = setInterval(()=>{
    const slides = $$(".banner-slide");
    if (!slides.length) return;
    bannerSlideIndex = (bannerSlideIndex + 1) % slides.length;
    showBannerSlide(bannerSlideIndex);
  }, 5000);
}
async function loadBannerSlideshow(forceRefetch=false){
  const wrap = $("#bannerSlider"), dots=$("#bannerDots");
  if (!wrap || !dots) return;
  wrap.querySelectorAll(".banner-slide").forEach(n=>n.remove());
  dots.innerHTML = "";
  try{
    const url = new URL(API.phimAll);
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res = await fetchBannerLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    const list = (json?.data ?? []).filter(m => (m.isHot==="1"||m.isHot===1||m.isHot===true) && !!m.banner);
    if (!list.length) return;
    list.forEach((movie, i)=>{
      const slide = document.createElement("div");
      slide.className = "banner-slide"+(i===0?" active":"");
      slide.style.backgroundImage = `url('${absUrl(movie.banner)}')`;
      slide.title = movie.tenPhim || "";
      slide.addEventListener("click", ()=> location.href = `../../pages/phim/details.html?id=${movie.maPhim}&lang=${encodeURIComponent(curAlias)}`);
      wrap.appendChild(slide);

      const dot = document.createElement("span");
      dot.className = "banner-dot"+(i===0?" active":"");
      dot.addEventListener("click", ()=> showBannerSlide(i));
      dots.appendChild(dot);
    });
    bannerSlideIndex = 0;
    startBannerAutoSlide();
  }catch(e){ if (e.name!=="AbortError") console.error(e); }
}

/* ================== MOVIES (Carousel 4) ================== */
function buildMoviesCarousel(movies){
  const host = $("#moviesGrid");
  host.innerHTML = `
    <div class="movies-carousel ${movies.length===1 ? "single":""}">
      <div class="carousel-viewport">
        <div class="carousel-track"></div>
      </div>
      <button class="carousel-btn prev" aria-label="Prev">❮</button>
      <button class="carousel-btn next" aria-label="Next">❯</button>
    </div>`;
  const track = $(".carousel-track", host);

  movies.forEach(m=>{
    const posterUrl = m.poster ? absUrl(m.poster) : '../../assets/images/default-poster.jpg';
    const card = document.createElement('div');
    card.className = 'movie-card';
    card.dataset.reveal = "30";
    card.innerHTML = `
      <div class="movie-poster" style="background-image:url('${posterUrl}')">
        ${m.trailer ? `<button class="trailer-overlay" data-trailer="${escapeHtml(m.trailer)}"></button>` : ``}
        <div class="info-reveal">
          <div class="info-title">${escapeHtml(m.tenPhim || tr("Movie_DefaultTitle","Phim"))}</div>
          <div class="info-actions">
            <button class="btn-green" data-buy="${m.maPhim}">${tr("Button_BuyTicket","Mua vé")}</button>
            <a class="btn-green-outline" href="../../pages/phim/details.html?id=${m.maPhim}&lang=${encodeURIComponent(curAlias)}">${tr("Link_ViewDetail","Xem chi tiết")}</a>
          </div>
        </div>
      </div>`;
    track.appendChild(card);
  });

  const prev = $(".carousel-btn.prev", host);
  const next = $(".carousel-btn.next", host);
  const cards = $$(".movie-card", host);

  function updateNav(){
    const visible = window.innerWidth < 900 ? 1 : window.innerWidth < 1200 ? 3 : 4;
    const totalPages = Math.max(1, Math.ceil(cards.length / visible));
    updateNav.page = Math.min(Math.max(updateNav.page||0, 0), totalPages-1);
    const cw = cards[0]?.getBoundingClientRect().width || 280;
    const gap = 18;
    const shift = updateNav.page * (cw*visible + gap*(visible-1));
    track.style.transform = `translateX(-${shift}px)`;
    prev.style.visibility = (updateNav.page>0) ? "visible" : "hidden";
    next.style.visibility = (updateNav.page<totalPages-1) ? "visible" : "hidden";
  }
  prev.onclick = ()=>{ updateNav.page=(updateNav.page||0)-1; updateNav(); };
  next.onclick = ()=>{ updateNav.page=(updateNav.page||0)+1; updateNav(); };
  window.addEventListener("resize", updateNav, { passive:true });
  updateNav();

  host.addEventListener("click", (e)=>{
    const trailerBtn = e.target.closest("[data-trailer]");
    if (trailerBtn){ e.preventDefault(); openTrailerModal(trailerBtn.dataset.trailer); return; }
    const buyBtn = e.target.closest("[data-buy]");
    if (buyBtn){
      const card = buyBtn.closest(".movie-card");
      const title = card?.querySelector(".info-title")?.textContent || "";
      const posterBg = card?.querySelector(".movie-poster")?.style?.backgroundImage || "";
      const poster = posterBg.replace(/^url\(["']?|["']?\)$/g,"");
      openBookModal({ maPhim: parseInt(buyBtn.dataset.buy,10), tenPhim: title, poster, meta: "" });
      return;
    }
  });
}
async function loadMovies(forceRefetch=false){
  const grid = $("#moviesGrid");
  if (!grid) return;
  grid.innerHTML = `<div class="loading" style="text-align:center;opacity:.8">${tr("Loading_Movies","Đang tải danh sách phim…")}</div>`;
  try{
    const url = new URL(API.phimAll);
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res = await fetchMoviesLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    const list = (json?.status && Array.isArray(json.data)) ? json.data : (Array.isArray(json) ? json : []);
    const movies = list.slice(0, 12);
    if (!movies.length){
      grid.innerHTML = `<p style="text-align:center;opacity:.8">${tr("NoData_Movies","Không có dữ liệu phim.")}</p>`;
      return;
    }
    buildMoviesCarousel(movies);
  }catch(err){
    if (err.name === 'AbortError') return;
    console.error(err);
    grid.innerHTML = `<div style="text-align:center;color:#ffb4b4">${tr("Error_Movies","Không thể tải danh sách phim.")}</div>`;
  }
}

/* ================== BOOKING MODAL ================== */
function ensureBookModal(){
  if ($('#bookModal')) return;
  const wrap = document.createElement('div');
  wrap.id = 'bookModal';
  wrap.className = 'modal';
  wrap.innerHTML = `
    <div class="modal-content">
      <div class="modal-header">
        <button id="bookClose" class="modal-close" aria-label="${tr("Detail_Button","Đóng")}">✕</button>
        <div class="form-group" style="display:flex;gap:14px;align-items:center;margin:0">
          <img id="bookPoster" src="" alt="poster" style="width:64px;height:92px;object-fit:cover;border-radius:8px;border:1px solid rgba(255,255,255,.12)">
          <div style="flex:1">
            <div id="bookMovieName" style="font-weight:800;font-size:1.1rem"></div>
            <div id="bookMovieMeta" style="opacity:.8"></div>
          </div>
          <button id="bookDetailBtn" class="btn-green-outline">${tr("Link_ViewDetail","Xem chi tiết")}</button>
        </div>
      </div>

      <div class="form-group">
        <label>${tr("Label_ShowDate","Ngày chiếu (29 ngày tới)")}</label>
        <div id="chipsDays" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr("Label_City","Thành phố")}</label>
        <div id="chipsCities" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr("Label_Cinema","Rạp")}</label>
        <div id="chipsRaps" class="chips"></div>
      </div>

      <div class="form-group">
        <label>${tr("Label_RoomShow","Phòng & Suất")}</label>
        <div id="roomShowtimes"></div>
      </div>
    </div>`;
  document.body.appendChild(wrap);
  const closeBookModal = ()=>{ wrap.style.display='none'; unlockBodyScroll(); };
  $('#bookClose').addEventListener('click', closeBookModal);
  wrap.addEventListener('click', (e)=>{ if (e.target === wrap) closeBookModal(); });
}
const state = { phim:null, dayKey:null, city:null, rapId:null, cities:[], raps:[] };

function formatYMD(d){ return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; }
function setActive(container, chip){
  container.querySelectorAll('.chip').forEach(c=>c.classList.remove('active'));
  chip.classList.add('active');
}
function renderDays(){
  const host = $('#chipsDays'); if (!host) return;
  host.innerHTML = '';
  const today = new Date();
  for (let i=0;i<29;i++){
    const d = new Date(today); d.setDate(d.getDate()+i);
    const chip = document.createElement('div');
    chip.className = 'chip' + (i===0 ? ' active' : '');
    chip.dataset.day = formatYMD(d);
    chip.textContent = `${String(d.getDate()).padStart(2,'0')}/${String(d.getMonth()+1).padStart(2,'0')}`;
    chip.addEventListener('click', ()=>{
      setActive(host, chip);
      state.dayKey = chip.dataset.day;
      if (state.city && state.rapId) loadShowtimes(true);
    });
    host.appendChild(chip);
    if (i===0) state.dayKey = chip.dataset.day;
  }
}
async function loadCities(forceRefetch=false){
  const host = $('#chipsCities'); if (!host) return;
  host.innerHTML = `<div class="no-data">${tr("Loading_Cities","Đang tải thành phố…")}</div>`;
  try{
    const url = new URL(API.cities);
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res = await fetchCitiesLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    state.cities = Array.isArray(json?.data) ? json.data : (Array.isArray(json) ? json : []);
    if (!state.cities.length){ host.innerHTML = `<span class="no-data">${tr("NoData_Cities","Không có danh sách thành phố.")}</span>`; return; }
    host.innerHTML = '';
    state.cities.forEach((city,idx)=>{
      const chip = document.createElement('div');
      chip.className = 'chip' + (idx===0 ? ' active' : '');
      chip.textContent = city;
      chip.addEventListener('click', ()=> selectCity(city, chip));
      host.appendChild(chip);
    });
    state.city = state.cities[0];
  }catch(e){
    if (e.name==='AbortError') return;
    console.error(e);
    host.innerHTML = `<span class="no-data">${tr("NoData_Cities_Alt","Lỗi tải danh sách thành phố.")}</span>`;
  }
}
function selectCity(city, chipEl){
  state.city = city;
  if (chipEl) setActive($('#chipsCities'), chipEl);
  loadRapsByCity(true).then(()=>{
    if (state.raps.length){
      const firstChip = $('#chipsRaps .chip');
      if (firstChip) selectRap(state.raps[0], firstChip);
    }else{
      const zone = $('#roomShowtimes'); if (zone) zone.innerHTML = `<span class="no-data">${tr("NoData_Cinemas","Không có rạp trong thành phố này.")}</span>`;
    }
  });
}
async function loadRapsByCity(forceRefetch=false){
  const host = $('#chipsRaps'); if (!host) return;
  host.innerHTML = `<div class="no-data">${tr("Loading_Cinemas","Đang tải rạp…")}</div>`;
  state.raps = []; state.rapId = null;
  try{
    const url = new URL(API.rapsByCity(state.city||''));
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res = await fetchRapsLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    state.raps = Array.isArray(json?.data) ? json.data : (Array.isArray(json) ? json : []);
    host.innerHTML = '';
    if (!state.raps.length){ host.innerHTML = `<span class="no-data">${tr("NoData_Cinemas","Không có rạp.")}</span>`; return; }
    state.raps.forEach((r, idx)=>{
      const chip = document.createElement('div');
      chip.className = 'chip' + (idx===0 ? ' active' : '');
      chip.textContent = r.tenRap || tr("Cinema_Number_Tpl",`Rạp #${r.maRap}`);
      chip.dataset.rapId = r.maRap;
      chip.addEventListener('click', ()=> selectRap(r, chip));
      host.appendChild(chip);
    });
    state.rapId = state.raps[0]?.maRap || null;
    if (state.rapId) loadShowtimes(true);
  }catch(e){
    if (e.name==='AbortError') return;
    console.error(e);
    host.innerHTML = `<span class="no-data">${tr("NoData_Cinemas","Lỗi tải rạp.")}</span>`;
  }
}
function selectRap(rap, chipEl){
  state.rapId = rap?.maRap ?? null;
  if (chipEl) setActive($('#chipsRaps'), chipEl);
  if (state.rapId) loadShowtimes(true);
}

/* ---- chuẩn hóa giờ ---- */
function normTimeStr(x){
  const raw = x?.gioChieu || x?.thoiGianBatDau || x?.ThoiGianBatDau || '';
  if (!raw) return '';
  if (/^\d{2}:\d{2}$/.test(raw)) return raw;
  const d = new Date(raw);
  if (!isNaN(d)) {
    const hh = String(d.getHours()).padStart(2,'0');
    const mm = String(d.getMinutes()).padStart(2,'0');
    return `${hh}:${mm}`;
  }
  return raw.slice(0,5);
}
const timeToMinutes = (hhmm)=> {
  const [h,m] = (hhmm||'').split(':').map(n=>parseInt(n,10));
  return (isNaN(h)||isNaN(m)) ? 0 : h*60+m;
};

async function loadShowtimes(forceRefetch=false){
  const host = $('#roomShowtimes');
  if (!host) return;
  host.innerHTML = `<div class="no-data">${tr("Loading_Showtimes","Đang tải suất chiếu…")}</div>`;
  if (!state.phim?.maPhim || !state.rapId || !state.dayKey){
    host.innerHTML = `<div class="no-data">${tr("Info_MissingFilters","Thiếu thông tin (ngày/thành phố/rạp).")}</div>`; return;
  }
  try{
    const url = new URL(API.suatByPhim(state.phim.maPhim, { maRap: state.rapId, ngay: state.dayKey }));
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res = await fetchShowtimesLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    const list = Array.isArray(json?.data) ? json.data : (Array.isArray(json) ? json : []);
    if (!list.length){ host.innerHTML = `<div class="no-data">${tr("NoData_Showtimes","Không có suất chiếu.")}</div>`; return; }

    const byRoom = new Map();
    for (const x of list){
      const roomId = x.maPhong ?? x.MaPhong ?? x.maPhongId ?? x.MaPhongId;
      const roomName = x.tenPhong ?? x.TenPhong ?? (tr("Room_Number_Tpl", `Phòng #${roomId}`));
      const arr = byRoom.get(roomId) || { roomId, roomName, items: [] };
      arr.items.push(x); byRoom.set(roomId, arr);
    }

    host.innerHTML = '';
    for (const {roomId, roomName, items} of byRoom.values()){
      const card = document.createElement('div');
      card.className = 'room-card';
      card.innerHTML = `
        <div class="room-head"><div class="room-name">🪑 ${escapeHtml(roomName)}</div></div>
        <div class="time-chips"></div>`;
      const zone = card.querySelector('.time-chips');

      items.sort((a,b)=> timeToMinutes(normTimeStr(a)) - timeToMinutes(normTimeStr(b)));
      items.forEach(s=>{
        const suatId = s.maSuat ?? s.MaSuat ?? s.id ?? s.Id;
        const time = normTimeStr(s) || '—';
        const chip = document.createElement('button');
        chip.className = 'time-chip';
        chip.textContent = time;
        chip.addEventListener('click', ()=>{
          const params = new URLSearchParams({ phimId: state.phim.maPhim, suatId, ngay: state.dayKey, rapId: state.rapId, roomId });
          location.href = `../../pages/booking/select-seats.html?${params.toString()}`;
        });
        zone.appendChild(chip);
      });
      host.appendChild(card);
    }
  }catch(e){
    if (e.name==='AbortError') return;
    console.error(e);
    host.innerHTML = `<div class="no-data">${tr("Error_Showtimes","Lỗi tải suất chiếu.")}</div>`;
  }
}
function openBookModal(phim){
  ensureBookModal();
  state.phim = phim;
  $('#bookPoster').src = phim.poster || '';
  $('#bookMovieName').textContent = phim.tenPhim || '';
  $('#bookMovieMeta').textContent = phim.meta || '';
  $('#bookDetailBtn').onclick = ()=> location.href = `../../pages/phim/details.html?id=${phim.maPhim}&lang=${encodeURIComponent(curAlias)}`;
  renderDays();
  loadCities(true).then(()=>{
    if (state.cities.length){
      selectCity(state.cities[0]);
    }else{
      const a = $('#chipsCities'); if (a) a.innerHTML = `<span class="no-data">${tr("NoData_Cities_Alt","Không có dữ liệu thành phố.")}</span>`;
      const b = $('#chipsRaps'); if (b) b.innerHTML = '';
      const c = $('#roomShowtimes'); if (c) c.innerHTML = '';
    }
  });
  const modal = $('#bookModal');
  modal.style.display = 'flex';
  lockBodyScroll();
}
window.openBookModal = openBookModal;

/* ================== Movie Detail (GET /api/phim/get/{id}?lang=) ================== */
function showDetailSkeleton(dom){
  dom.innerHTML = `<div class="detail-skeleton" style="padding:20px;opacity:.75">${tr("Loading_Detail","Đang tải chi tiết…")}</div>`;
}
async function loadMovieDetailInto(dom, forceRefetch=false){
  const id = Number(getQuery('id'));
  const langQ = getQuery('lang');
  const lang = CULT[langQ] ? langQ : curAlias;
  if (lang !== curAlias){
    curAlias = lang; localStorage.setItem("lang", curAlias);
    await i18nLoad(curAlias); applyI18nToDom(document);
  }
  if (!id || !dom) return;

  showDetailSkeleton(dom); // cập nhật UI tức thì

  try{
    const url = new URL(API.phimById(id, lang));
    if (forceRefetch) url.searchParams.set('_', Date.now());
    const res  = await fetchDetailLatest(url.toString(), { headers:{'Accept':'application/json'}, cache:'no-store' });
    const json = await res.json();
    const dto  = json?.data || {};

    const poster = dto.poster ? absUrl(dto.poster) : '../../assets/images/default-poster.jpg';
    const banner = dto.banner ? absUrl(dto.banner) : '';

    dom.innerHTML = `
      <div class="detail-hero" style="${banner ? `--bg:url('${banner}')` : ''}">
        <div class="detail-hero-overlay">
          <img class="detail-poster" src="${poster}" alt="poster" />
          <div class="detail-meta">
            <h1 class="detail-title">${escapeHtml(dto.tenPhim || '')}</h1>
            <div class="detail-info">
              ${dto.thoiLuong ? `<span>⏱ ${dto.thoiLuong}’</span>` : ''}
              ${dto.daoDien ? `<span>🎬 ${escapeHtml(dto.daoDien)}</span>` : ''}
              ${dto.ngonNgu ? `<span>🌐 ${escapeHtml(dto.ngonNgu)}</span>` : ''}
              ${Array.isArray(dto.loaiPhims) && dto.loaiPhims.length
                 ? `<span>🏷 ${dto.loaiPhims.map(lp=>escapeHtml(lp.tenLoaiPhim||'')).join(', ')}</span>` : ''}
            </div>
            ${dto.trailer ? `<button class="btn-green" id="detailTrailerBtn">${tr("Button_Trailer","Xem trailer")}</button>` : ''}
            <button class="btn-green-outline" id="detailBuyBtn">${tr("Button_BuyTicket","Mua vé")}</button>
          </div>
        </div>
      </div>

      <div class="detail-body">
        <h2>${tr("Label_Overview","Tổng quan")}</h2>
        <p>${escapeHtml(dto.moTa || tr("NoDesc","Chưa có mô tả."))}</p>
      </div>
    `;

    const trailerBtn = document.getElementById('detailTrailerBtn');
    if (trailerBtn && dto.trailer){ trailerBtn.addEventListener('click', ()=>openTrailerModal(dto.trailer)); }

    const buyBtn = document.getElementById('detailBuyBtn');
    if (buyBtn){
      buyBtn.addEventListener('click', ()=>{
        openBookModal({ maPhim: dto.maPhim, tenPhim: dto.tenPhim, poster, meta: '' });
      });
    }
  }catch(err){
    if (err.name==='AbortError') return;
    console.error(err);
    dom.innerHTML = `<div class="no-data" style="text-align:center;color:#ffb4b4">${tr("Error_Detail","Không thể tải chi tiết phim.")}</div>`;
  }
}

/* ================== MoMo callback ================== */
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
    fetch(`${BASE_URL}/api/momopayment/confirm?_=${Date.now()}`, {
      method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload), cache:'no-store'
    }).catch(()=>{}).finally(()=>{ history.replaceState({}, "", location.pathname + location.hash); });
  } catch {
    try{ history.replaceState({}, "", location.pathname + location.hash); }catch{}
  }
})();

/* ================== BOOT ================== */
document.addEventListener('DOMContentLoaded', async ()=>{
  const langQ = getQuery('lang');
  if (CULT[langQ]) { curAlias = langQ; localStorage.setItem("lang", curAlias); }

  await i18nLoad(curAlias);
  try{ sessionStorage.removeItem("BOOKING_TAB"); }catch{}
  try{ sessionStorage.removeItem("BOOKING_FLOW"); }catch{}
  includeHTML("include-header", "../../part/header.html");
  includeHTML("include-footer", "../../part/footer.html");
  applyI18nToDom(document);

  // Refetch bảo đảm latest-wins
  try{ loadMovies(true); }catch{}
  try{ loadBannerSlideshow(true); }catch{}

  const detailMount = document.getElementById('movieDetail');
  if (detailMount){
    await loadMovieDetailInto(detailMount, true);
  }
});
/* ================== PAYMENT CALLBACK (MoMo + VNPAY) ================== */
(function handlePaymentCallbacks() {
  try {
    const saved = sessionStorage.getItem('PAY_CALLBACK_QS');
    if (!saved) return;
    if (window.__PAY_CB_SENT) return;
    window.__PAY_CB_SENT = true; // tránh gửi lặp

    const q = new URLSearchParams(saved);

    // ---- MoMo ----
    const isMomo = q.get('orderId') && q.get('resultCode') && q.get('signature');
    if (isMomo) {
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
      fetch(`${BASE_URL}/api/momopayment/confirm?_=${Date.now()}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        cache: 'no-store'
      }).catch(()=>{}).finally(()=>{
        try { sessionStorage.removeItem('PAY_CALLBACK_QS'); } catch {}
        // // Optional: điều hướng trang thành công
        // location.href = '../../pages/booking/success.html';
      });
      return;
    }

    // ---- VNPAY ----
    const isVnp = Array.from(q.keys()).some(k => k.toLowerCase().startsWith('vnp_'));
    if (isVnp) {
      const qs = q.toString();

      // 1) Ưu tiên /return để BE xác thực + cập nhật DB
      fetch(`${BASE_URL}/api/payments/vnpay/return?${qs}&_=${Date.now()}`, {
        method: 'GET', cache: 'no-store'
      })
      .then(r => r.ok ? r.json() : null)
      .then(ret => {
        const ok = ret && (
          ret.RspCode === '00' ||
          ret.vnp_ResponseCode === '00' ||
          ret.responseCode === '00' ||
          ret.isSuccess === true
        );
        if (ok) return; // done

        // 2) Fallback /ipn nếu /return chưa OK
        return fetch(`${BASE_URL}/api/payments/vnpay/ipn?${qs}&_=${Date.now()}`, {
          method: 'GET', cache: 'no-store'
        }).then(r => r.ok ? r.json() : null);
      })
      .finally(() => {
        try { sessionStorage.removeItem('PAY_CALLBACK_QS'); } catch {}
        // // Optional: điều hướng trang thành công
        // location.href = '../../pages/booking/success.html';
      });
    }
  } catch {}
})();
