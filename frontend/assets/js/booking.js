/* =================== CONFIG =================== */
const BASE_URL = "https://localhost:7058";
const phimId   = new URLSearchParams(location.search).get("phimId");

/* =================== STATE ==================== */
const suatChieuMap    = {};        // maSuat(string) -> maPhong
let   selectedSeatIds = new Set(); // ghế user hiện tại đang giữ
let   seatPrices      = {};        // maGhe (number) -> giá vé (number)
let   selectedVeIds   = {};        // maGhe -> maVe (từ /hold)
let   currentUserId   = null;
let   currentBapPrice = 0;

let   seatHub       = null;        // SignalR connection
let   currentSuatId = null;        // maSuat đang xem (string)
let   seatEls       = new Map();   // cache maGhe (number) -> element
let   seatMeta      = new Map();   // maGhe -> { row, num } (để FE validate rule)
let   viewToken     = 0;           // chống race khi đổi suất
let   httpAbort     = null;        // AbortController cho fallback HTTP
let   bookingInFlight = false;     // chặn double-click/đúp event

/* =================== BUTTON STATE =================== */
const BTN_TEXT = {
  idle: "🎟 Thanh toán ngay",
  ready: "🎟 Thanh toán ngay",               // ✅ có 'ready'
  creatingInvoice: "⏳ Đợi: đang tạo hóa đơn…",
  creatingPayment: "⏳ Đợi: đang tạo thanh toán MoMo…",
  redirecting: "⏳ Đợi: đang chuyển đến MoMo…",
  error: "❌ Thử lại"
};
function setBtnState(state) {
  const btn = document.querySelector(".btn-book");
  if (!btn) return;

  btn.textContent = BTN_TEXT[state] ?? BTN_TEXT.idle;

  const isLoading = state === "creatingInvoice" ||
                    state === "creatingPayment" ||
                    state === "redirecting";
  btn.disabled = isLoading ? true : false;
  btn.classList.toggle("is-loading", isLoading);
}
/* =================== HELPERS =================== */
const token    = () => localStorage.getItem("token");
const numberVN = n => { try { return Number(n).toLocaleString("vi-VN"); } catch { return n; } };
const normalizeId = x => { const n = parseInt(x, 10); return isNaN(n) ? null : n; };
function uuid(){ try{ return crypto.randomUUID(); } catch { return `${Date.now()}-${Math.random().toString(36).slice(2)}`; } }
function getOrCreateClientToken() {
  if (!sessionStorage.clientToken) {
    sessionStorage.clientToken = (crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(36).slice(2)}`);
  }
  return sessionStorage.clientToken;
}

async function getJson(url, options = {}) {
  const res = await fetch(url, options);
  let data = null; try { data = await res.json(); } catch {}
  if (!res.ok || data?.status === false) throw new Error(data?.message || `HTTP ${res.status}`);
  return data;
}
async function postJson(url, body, options = {}) {
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type":"application/json", ...(options.headers||{}) },
    body: body == null ? null : JSON.stringify(body),
    signal: options.signal
  });
  let data = null; try { data = await res.json(); } catch {}
  if (!res.ok || data?.status === false) throw new Error(data?.message || `HTTP ${res.status}`);
  return data ?? { status: true };
}
const authHeaders = () => token() ? { "Authorization": "Bearer " + token() } : {};

function getUserIdFromToken(tk){
  try{
    const parts=(tk||"").split('.'); if(parts.length<2) return null;
    const b=parts[1].replace(/-/g,'+').replace(/_/g,'/');
    const json=decodeURIComponent(atob(b).split('').map(c=>'%' + ('00'+c.charCodeAt(0).toString(16)).slice(-2)).join(''));
    const p=JSON.parse(json);
    return p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || p["UserId"] || null;
  }catch{ return null; }
}

/* =================== MOVIE INFO =================== */
getJson(`${BASE_URL}/api/phim/get/${phimId}`)
  .then(res=>{
    const container = document.getElementById("movieInfo");
    if(!res.status){ container.textContent="Không tìm thấy phim"; return; }
    const p = res.data;
    const poster = p.poster? BASE_URL+p.poster: "../../assets/images/default-poster.jpg";
    container.innerHTML = `
      <h2></h2>
      <img alt="">
      <div class="movie-details">
        <p><strong>🎭 Thể loại:</strong> <span class="genres"></span></p>
        <p><strong>⏱️ Thời lượng:</strong> <span class="duration"></span></p>
      </div>`;
    container.querySelector("h2").textContent = p.tenPhim || "Phim";
    const img = container.querySelector("img");
    img.src = poster; img.alt = p.tenPhim || "Poster";
    container.querySelector(".genres").textContent = (p.loaiPhims?.map(l=>l.tenLoaiPhim).join(", ")) || "N/A";
    container.querySelector(".duration").textContent = `${p.thoiLuong||0} phút`;
  })
  .catch(err=>console.error("Lỗi tải thông tin phim:", err));

/* =================== SUẤT CHIẾU =================== */
getJson(`${BASE_URL}/api/suatchieu/get-by-phim/${phimId}`)
  .then(res=>{
    const sel = document.getElementById("suatChieu");
    if(!res.status||!res.data?.length){
      sel.innerHTML='<option>❌ Không có suất chiếu</option>';
      return;
    }

    res.data.forEach(s=>{
      suatChieuMap[String(s.maSuat)] = s.maPhong;
      const opt = document.createElement("option");
      opt.value=s.maSuat;
      opt.textContent=`📅 ${s.ngayChieu} - ⏰ ${s.gioChieu}`;
      sel.append(opt);
    });

    // gắn event change (render ghế)
    sel.addEventListener("change", loadSeats);

    // ✅ Auto-chọn suất 2 nếu có; nếu không thì suất 1
    if (res.data.length >= 2) {
      sel.value = String(res.data[1].maSuat);
    } else {
      sel.value = String(res.data[0].maSuat);
    }
    // Đồng bộ state từ DOM
    currentSuatId = String(sel.value);

    // Phát change để kích hoạt mọi listener + loadSeats
    sel.dispatchEvent(new Event('change'));
  })
  .catch(err=>console.error("Lỗi tải suất chiếu:", err));

/* =================== BẮP NƯỚC =================== */
getJson(`${BASE_URL}/api/bapnuoc/get-all`)
  .then(res=>{
    const sel=document.getElementById("bapNuoc");
    if(res.status){
      res.data.forEach(b=>{
        const opt=document.createElement("option");
        opt.value=b.maBn;
        opt.dataset.price=b.gia;
        opt.textContent=`🍿 ${b.tenBn} - ${numberVN(b.gia)}đ`;
        sel.append(opt);
      });
    }
    sel.addEventListener("change", updateTotalPrice);
  })
  .catch(err=>console.error("Lỗi tải bắp nước:", err));

/* ========== SEAT UI HELPERS ========== */
function markBooked(el){ el.classList.remove('selected'); el.classList.add('booked'); el.style.pointerEvents='none'; }
function markSelected(el){ el.classList.remove('booked'); el.classList.add('selected'); el.style.pointerEvents='auto'; }
function markFree(el){ el.classList.remove('booked','selected'); el.style.pointerEvents='auto'; }

/* ========== ÁP DỤNG TRẠNG THÁI GHẾ ========== */
function applySeatState({ maSuat, maGhe, trangThai, holderUserId }) {
  const selSuatEl = document.getElementById('suatChieu');
  const selSuat   = String(selSuatEl?.value || "");
  // ✅ chấp nhận nếu trùng DOM HOẶC currentSuatId
  if (String(maSuat) !== selSuat && String(maSuat) !== String(currentSuatId || "")) return;

  const id = normalizeId(maGhe);
  if (id == null) return;

  const el = seatEls.get(id);
  if (!el) return;

  el.classList.remove('booked', 'selected', 'pending');
  el.style.pointerEvents = 'auto';

  const t = (trangThai || '').trim();

  // Lưu dataset để validator có thể đọc
  el.dataset.state  = t || 'Trong';
  el.dataset.holder = holderUserId ?? '';
  el.dataset.suat   = String(maSuat); // <<< thêm data-suat

  if (t === '' || t === 'Trong') {
    selectedSeatIds.delete(id);
    delete selectedVeIds[id];
    delete seatPrices[id];
    markFree(el);
    el.onclick = () => toggleSeat(el, id, normalizeId(maSuat));
    renderSeatSummary();
    return;
  }

  if (t === 'TamGiu') {
    if (holderUserId && currentUserId && holderUserId === currentUserId) {
      markSelected(el);
      selectedSeatIds.add(id);
      el.onclick = () => toggleSeat(el, id, normalizeId(maSuat));
      renderSeatSummary();
    } else {
      markBooked(el);
      el.onclick = null;
    }
    return;
  }

  if (t === 'DaDat') {
    markBooked(el);
    el.onclick = null;
    selectedSeatIds.delete(id);
    delete selectedVeIds[id];
    delete seatPrices[id];
    renderSeatSummary();
  }
}

/* =================== RENDER 1 GHẾ =================== */
// ✅ FIXED: parseSeatLabel
function parseSeatLabel(txt){
  if (!txt) return { row: '', num: -1 };
  const s = String(txt);
  const rowMatch = s.match(/[A-Za-z]+/);
  const numMatch = s.match(/\d+/);
  const row = (rowMatch ? rowMatch[0] : '').toUpperCase();
  const num = numMatch ? parseInt(numMatch[0], 10) : -1;
  return { row, num: Number.isFinite(num) ? num : -1 };
}

function renderSeat(grid, ghe, suatId){
  const div = document.createElement('div');
  div.className   = 'seat';
  div.textContent = ghe.soGhe || ghe.tenGhe || '(?)';

  const maGhe = normalizeId(ghe.maGhe ?? ghe.maGheId ?? ghe.id);
  if (maGhe == null) return;

  // dùng data-id + data-suat để validator/handlers đọc
  div.setAttribute('data-id', String(maGhe));
  div.dataset.suat = String(suatId);

  grid.appendChild(div);
  seatEls.set(maGhe, div);

  const meta = parseSeatLabel(ghe.soGhe || ghe.tenGhe || '');
  seatMeta.set(maGhe, meta);

  applySeatState({
    maSuat: normalizeId(suatId),
    maGhe: maGhe,
    trangThai: (ghe.trangThai ?? ghe.TrangThai ?? '').trim(),
    holderUserId: ghe.holderUserId ?? ghe.HolderUserId ?? null
  });

  // Gán handler sau khi set trạng thái
  div.onclick = () => toggleSeat(div, maGhe, normalizeId(suatId));
}

/* =================== SIGNALR (snapshot + realtime) =================== */
async function ensureSignalR() {
  if (window.signalR) return;
  await new Promise((resolve, reject) => {
    const s = document.createElement('script');
    s.src = "https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.5/dist/browser/signalr.min.js";
    s.onload = resolve;
    s.onerror = () => reject(new Error("Không load được SignalR client"));
    document.head.appendChild(s);
  });
}

async function connectSeatHub(maSuat){
  await ensureSignalR();

  // Đóng connection cũ nếu có
  if (seatHub) {
    try { await seatHub.stop(); } catch {}
    seatHub = null;
  }

  seatHub = new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL.replace(/\/+$/,'')}/seatHub`, {
      accessTokenFactory: () => localStorage.getItem("token") || ""
    })
    .withAutomaticReconnect()
    .build();

  seatHub.on("SeatsSnapshot", (list) => {
    const selSuat = String(document.getElementById('suatChieu').value||"");
    // ✅ chỉ bỏ qua nếu KHÔNG trùng cả DOM lẫn state
    if (String(maSuat) !== selSuat && String(maSuat) !== String(currentSuatId)) return;

    const grid = document.getElementById('seatGrid');
    if (!grid) return;

    grid.innerHTML = '';
    seatEls.clear();
    seatMeta.clear();
    list.forEach(ghe => renderSeat(grid, ghe, maSuat));
  });

  seatHub.on("SeatUpdated", (p) => {
    const payload = {
      maSuat:     normalizeId(p.maSuat ?? p.MaSuat),
      maGhe:      normalizeId(p.maGhe ?? p.MaGhe),
      trangThai: (p.trangThai ?? p.TrangThai ?? '').trim(),
      holderUserId: p.holderUserId ?? p.HolderUserId ?? null
    };
    applySeatState(payload);
  });

  seatHub.onreconnected(async () => {
    try {
      if (currentSuatId) await seatHub.invoke("JoinShowtime", parseInt(currentSuatId,10));
      if (seatHub.invoke && currentSuatId) {
        try { await seatHub.invoke("RequestSnapshot", parseInt(currentSuatId,10)); } catch {}
      }
    } catch {}
  });

  await seatHub.start();
  await seatHub.invoke("JoinShowtime", parseInt(maSuat, 10));
  // ✅ yêu cầu snapshot ngay (nếu server có method)
  try { await seatHub.invoke("RequestSnapshot", parseInt(maSuat, 10)); } catch {}
}

/* =================== LOAD & RENDER GHẾ =================== */
function loadSeats(){
  currentUserId = getUserIdFromToken(token());
  const suatId  = String(document.getElementById('suatChieu').value);
  const maPhong = suatChieuMap[String(suatId)];
  const grid    = document.getElementById('seatGrid');

  const myToken = ++viewToken;

  // set current showtime & dừng hub cũ (nếu có)
  currentSuatId = suatId;
  if (seatHub) {
    try { seatHub.stop(); } catch {}
    seatHub = null;
  }

  seatPrices = {};
  renderSeatSummary();

  grid.innerHTML = '<div class="loading">Đang tải ghế...</div>';
  if (!maPhong){
    grid.innerHTML = '<div class="loading">Không tìm thấy phòng từ suất chiếu.</div>';
    return;
  }

  connectSeatHub(suatId).catch(err=>console.error("SeatHub error:", err));

  if (httpAbort) { try { httpAbort.abort(); } catch {} }
  httpAbort = new AbortController();

  const url = `${BASE_URL}/api/ghe/get-trang-thai?maPhong=${maPhong}&maSuat=${suatId}&_=${Date.now()}`;
  getJson(url, { headers: authHeaders(), signal: httpAbort.signal })
    .then(res=>{
      if (viewToken !== myToken) return;
      if (!grid.querySelector('.loading')) return;

      grid.innerHTML='';
      seatEls.clear();
      seatMeta.clear();
      if(!res.status || !Array.isArray(res.data) || !res.data.length){
        grid.innerHTML='<p style="color:#ff6b6b;text-align:center;padding:40px;">❌ Không có ghế khả dụng.</p>';
        return;
      }
      res.data.forEach(ghe=>renderSeat(grid,ghe,suatId));
    })
    .catch(err=>{
      if (err?.name === 'AbortError') return;
      console.error('Lỗi khi lấy ghế:',err);
      if (viewToken === myToken && grid.querySelector('.loading')) {
        grid.innerHTML='<p style="color:#ff6b6b;text-align:center;padding:40px;">❌ Lỗi kết nối máy chủ.</p>';
      }
    })
    .finally(()=>{ httpAbort = null; });
}

function validateNoLonelySeatOnClient(trySeatId, suatIdOverride) {
  const currentSuat = String(
    suatIdOverride ?? currentSuatId ?? document.getElementById('suatChieu')?.value ?? ''
  );

  const t = seatMeta.get(trySeatId);
  if (!t || !t.row || !Number.isInteger(t.num) || t.num <= 0) return true;

  // Thu thập toàn bộ ghế cùng dãy & cùng suất
  const rowItems = [];
  for (const [id, m] of seatMeta.entries()) {
    if (!m || m.row !== t.row || !Number.isInteger(m.num) || m.num <= 0) continue;
    const el = seatEls.get(id);
    if (!el) continue;
    const sameSuat = !el.dataset?.suat || String(el.dataset.suat) === currentSuat;
    if (sameSuat) rowItems.push({ id, n: m.num, el });
  }
  if (!rowItems.length) return true;
  rowItems.sort((a,b) => a.n - b.n);

  const byN  = new Map(rowItems.map(x => [x.n, x]));
  const minN = rowItems[0].n;
  const maxN = rowItems[rowItems.length-1].n;

  // Trạng thái hiện tại
  const isHard = (n) => {
    const it = byN.get(n);
    if (!it) return false;
    return it.el.classList.contains('booked') || it.el.dataset.state === 'TamGiu';
  };
  const isSoftNow = (n) => {
    const it = byN.get(n);
    return it ? selectedSeatIds.has(it.id) : false;
  };
  const isAnyNow  = (n) => isHard(n) || isSoftNow(n);
  const isFreeNow = (n) => byN.has(n) && !isAnyNow(n);

  // ✅ NGOẠI LỆ MỚI: cặp 2 ghế trống kẹp giữa ANY -> cho phép chọn 1 ghế
  // Tìm cụm trống hiện tại chứa ghế đang click
  let L = t.num, R = t.num;
  while (isFreeNow(L-1)) L--;
  while (isFreeNow(R+1)) R++;
  const segLen = R - L + 1;
  const hasLeftBoundary  = byN.has(L-1);
  const hasRightBoundary = byN.has(R+1);
  const leftBoundaryAny  = hasLeftBoundary  && isAnyNow(L-1);
  const rightBoundaryAny = hasRightBoundary && isAnyNow(R+1);

  // Nếu cụm trống dài đúng 2 ghế và hai đầu là ANY => cho phép (return true sớm)
  if (segLen === 2 && leftBoundaryAny && rightBoundaryAny) {
    return true;
  }

  // ========== PHẦN LOGIC CŨ (giữ nguyên) ==========
  const isSoftAfter = (n) => {
    const it = byN.get(n);
    if (!it) return false;
    const now = selectedSeatIds.has(it.id);
    return it.id === trySeatId ? !now : now; // toggle ghế đang click
  };
  const isAnyAfter  = (n) => isHard(n) || isSoftAfter(n);
  const isFreeAfter = (n) => byN.has(n) && !isAnyAfter(n);

  const allAnyFromTo = (a, b) => {
    if (a > b) [a, b] = [b, a];
    for (let k = a; k <= b; k++) {
      if (!byN.has(k)) continue;
      if (!isAnyAfter(k)) return false;
    }
    return true;
  };

  // Quét TẤT CẢ ghế trong dãy
  for (const { n } of rowItems) {
    if (!isFreeAfter(n)) continue;

    const hasLeft  = byN.has(n-1);
    const hasRight = byN.has(n+1);
    const leftAny  = hasLeft  && isAnyAfter(n-1);
    const rightAny = hasRight && isAnyAfter(n+1);

    // 1) Ở GIỮA DÃY: ANY – FREE – ANY => CHẶN
    if (hasLeft && hasRight && leftAny && rightAny) {
      throw new Error(`Không thể để lẻ ghế ${t.row}${n}.`);
    }

    // 2) ĐẦU DÃY
    if (!hasLeft && rightAny) {
      const allInsideAny = allAnyFromTo(n+1, maxN);
      if (allInsideAny) continue;
      if (!(byN.has(n+2) && isHard(n+2))) {
        throw new Error(`Không thể để lẻ ghế ${t.row}${n} ở đầu dãy.`);
      }
    }

    // 3) CUỐI DÃY
    if (!hasRight && leftAny) {
      const allInsideAny = allAnyFromTo(minN, n-1);
      if (allInsideAny) continue;
      if (!(byN.has(n-2) && isHard(n-2))) {
        throw new Error(`Không thể để lẻ ghế ${t.row}${n} ở cuối dãy.`);
      }
    }
  }

  return true;
}

/* ================== HOLD / RELEASE SEAT ================== */
async function toggleSeat(div, maGhe, maSuat){
  if (!token()) { alert("⚠️ Vui lòng đăng nhập."); return; }
  const seatId = normalizeId(maGhe);
  const suatId = normalizeId(maSuat);
  if (seatId == null || suatId == null) return;

  const isCurrentlySelected = div.classList.contains('selected');
  if (!isCurrentlySelected && selectedSeatIds.size >= 8){
    alert("⚠️ Mỗi đơn chỉ giữ tối đa 8 ghế.");
    return;
  }

  if (!isCurrentlySelected){
    try{
      validateNoLonelySeatOnClient(seatId, suatId); // <<< truyền suất
    }catch(ruleErr){
      div.classList.add('invalid'); 
      setTimeout(()=>div.classList.remove('invalid'), 600);
      alert(ruleErr.message || "Không thể để lẻ 1 ghế. Vui lòng chọn lại.");
      return;
    }
  }

  div.classList.add('pending');
  try {
    if (isCurrentlySelected) {
      await postJson(`${BASE_URL}/api/ve/release`, { maGhe:seatId, maSuat:suatId }, { headers: authHeaders() });
      selectedSeatIds.delete(seatId);
      delete selectedVeIds[seatId];
      delete seatPrices[seatId];
      renderSeatSummary();
      markFree(div);
      div.onclick = () => toggleSeat(div, seatId, suatId);
      return;
    }

    const resp = await postJson(`${BASE_URL}/api/ve/hold`, { maGhe:seatId, maSuat:suatId }, { headers: authHeaders() });
    const ve = resp.data || {};
    selectedSeatIds.add(seatId);
    if (ve.maVe != null) selectedVeIds[seatId] = ve.maVe;

    if (ve.giaVe != null) {
      seatPrices[seatId] = parseInt(ve.giaVe, 10);
    } else {
      try{
        const gia = await getJson(`${BASE_URL}/api/ve/tinh-gia-ve?maGhe=${seatId}&maSuat=${suatId}`, { headers: authHeaders() });
        if (gia?.data?.giaCuoiCung != null) seatPrices[seatId] = parseInt(gia.data.giaCuoiCung, 10);
      }catch(e){
        console.warn("Không lấy được giá ghế, sẽ để 0 tạm:", e);
      }
    }

    markSelected(div);
    div.onclick = () => toggleSeat(div, seatId, suatId);
    renderSeatSummary();
  } catch (e) {
    console.error("Hold seat lỗi:", e);
    const msg = e?.message ? String(e.message) : "";

    const isRuleViolation = /l[eê] gh[eế]|lonely\s*seat|quy t[ăa]c/i.test(msg);
    const isTakenByOthers = /gh[eế].*(gi[uữ]|đ[aă]t)|reserved|occupied/i.test(msg);

    if (isRuleViolation) {
      markFree(div);
      alert(msg || "Không thể để lẻ 1 ghế. Vui lòng chọn lại.");
    } else if (isTakenByOthers) {
      markBooked(div);
      alert(msg || "Ghế đã được giữ/đặt bởi người khác.");
    } else {
      alert(msg || "Có lỗi khi giữ ghế. Vui lòng thử lại.");
    }
  } finally {
    div.classList.remove('pending');
  }
}

/* =================== SUMMARY & TOTAL =================== */
function renderSeatSummary(){
  const info=document.getElementById("seatPriceInfo");
  if (info) {
    info.innerHTML=[...selectedSeatIds].map(id=>`🪑 ${id}: ${numberVN(seatPrices[id]||0)}đ`).join("<br>")||"";
  }
  updateTotalPrice();
}
function updateTotalPrice(){
  const bapSel=document.getElementById("bapNuoc");
  currentBapPrice=parseInt(bapSel?.selectedOptions?.[0]?.dataset?.price || 0);
  const seatSum=Object.values(seatPrices).reduce((t,p)=>t+(parseInt(p||0,10)||0),0);
  const el = document.getElementById("totalPrice");
  if (el) el.textContent=numberVN(seatSum+currentBapPrice)+"đ";
}
function getSelectedSeatsArray() { return [...selectedSeatIds].map(id => id); }
function calcSeatSum() { return Object.values(seatPrices).reduce((t,p)=>t + (parseInt(p||0,10)||0), 0); }

async function createInvoiceOnServer({ maSuat, seatIds, bapNuocId, soLuongBapNuoc }) {
  // Tách riêng veLines và bnLines
  const veLines = [];
  for (const seatId of seatIds) {
    const veId = selectedVeIds[seatId];
    if (veId != null) veLines.push({ maVe: Number(veId), soLuong: 1 });
  }

  const bnLines = [];
  if (bapNuocId) {
    bnLines.push({ maBn: Number(bapNuocId), soLuong: soLuongBapNuoc ?? 1 });
  }

  // Chỉ dùng nhánh maVe khi TẤT CẢ ghế đã có maVe
  const useVePath = veLines.length > 0 && veLines.length === seatIds.length;

  const body = {
    maSuat: Number(maSuat),
    seatIds: useVePath ? [] : seatIds.map(Number),
    bapNuocId: bapNuocId ? Number(bapNuocId) : null,
    soLuongBapNuoc: bapNuocId ? (soLuongBapNuoc ?? 1) : 0,
    chiTietHoaDons: useVePath ? [...veLines, ...bnLines] : [...bnLines],
    ghiChu: "Đặt vé qua website",
    hinhThucThanhToan: "Momo"
  };

  const res = await postJson(`${BASE_URL}/api/hoadon/create`, body, { headers: authHeaders() });
  const data = res?.data ?? res;

  return {
    maHd: data.maHd ?? data.MaHd ?? data.id ?? data.orderId,
    amount: Number(data.tongTien ?? data.amount ?? 0),
    orderInfo: data.orderInfo,
    fullName: data.fullName
  };
}

/* =================== MoMo PAYMENT =================== */
async function createMomoPayment({ orderId, amount, orderInfo, fullName }) {
  const body = {
    orderId: String(orderId),
    amount: Number(amount || 0),
    orderInfo: orderInfo || `Thanh toán vé phim - HĐ ${orderId}`,
    fullName:  fullName  || "Khách hàng"
  };
  const res = await postJson(`${BASE_URL}/api/momopayment/create`, body, { headers: authHeaders() });
  const url = res?.payUrl || res?.shortLink || res?.deeplink;
  if (!url) throw new Error("Không nhận được payUrl từ MoMo.");
  return url;
}

/* =================== BOOK (HĐ -> MoMo) =================== */
async function bookTicket(){
  if (bookingInFlight) return;
  bookingInFlight = true;

  try {
    if (!token()) throw new Error("⚠️ Vui lòng đăng nhập trước khi đặt vé.");

    const suatEl = document.getElementById("suatChieu");
    const maSuat = String(suatEl?.value||"");
    const seatIds = getSelectedSeatsArray();

    if (!maSuat) throw new Error("⚠️ Vui lòng chọn suất chiếu.");
    if (!seatIds.length) throw new Error("⚠️ Vui lòng chọn ít nhất 1 ghế.");

    const bapSel = document.getElementById("bapNuoc");
    const bapNuocId = bapSel?.value ? Number(bapSel.value) : null;

    setBtnState("creatingInvoice");

    // 1) Tạo hóa đơn ở BE
    const invoice = await createInvoiceOnServer({
      maSuat,
      seatIds,
      bapNuocId,
      soLuongBapNuoc: 1
    });

    const maHd = invoice?.maHd;
    if (!maHd) throw new Error("Không nhận được mã hóa đơn (maHd).");

    // 2) Tạo thanh toán MoMo — dùng amount từ BE (đã snapshot)
    setBtnState("creatingPayment");
    // FE - tạo payment
    const amount = Math.round(Number(invoice.amount || 0)); // đảm bảo integer
    if (!amount || amount <= 0) throw new Error("Số tiền thanh toán không hợp lệ.");

    const payUrl = await createMomoPayment({
      orderId: String(maHd),
      amount, // <-- integer VND
      orderInfo: invoice?.orderInfo,
      fullName:  invoice?.fullName
    });

    // 3) Redirect sang MoMo
    setBtnState("redirecting");
    location.href = payUrl;

  } catch (err) {
    console.error("Đặt vé lỗi:", err);
    alert(err?.message || "Có lỗi khi đặt vé. Vui lòng thử lại.");
    setBtnState("error");
  } finally {
    bookingInFlight = false;
  }
}

/* =================== EVENT LISTENERS =================== */
document.addEventListener("DOMContentLoaded", () => {
  const $ = (s, r=document) => r.querySelector(s);
  const $$ = (s, r=document) => Array.from(r.querySelectorAll(s));

  // Map DOM seat elements vào seatEls (nếu có sẵn từ SSR)
  if (typeof seatEls !== "undefined" && seatEls.size === 0) {
    $$(".seat[data-id]").forEach(el => {
      const id = Number(el.dataset.id);
      if (!Number.isNaN(id)) seatEls.set(id, el);
    });
  }

  // ====== dropdowns ======
  const suatChieuEl = $("#suatChieu");
  if (suatChieuEl) {
    suatChieuEl.addEventListener("change", () => {
      if (typeof selectedSeatIds !== "undefined") {
        selectedSeatIds.clear();
      }
      $$(".seat.selected").forEach(el => el.classList.remove("selected"));
      updateTotalPrice?.();
      updateBookBtnState();
    });
  }

  const bapNuocEl = $("#bapNuoc");
  if (bapNuocEl) {
    bapNuocEl.addEventListener("change", () => {
      updateTotalPrice?.();
      updateBookBtnState();
    });
  }

  // (Giữ handler click gắn trực tiếp trên từng ghế trong renderSeat)

  // ====== nút đặt vé ======
  const bookBtn = document.querySelector(".btn-book");
  function updateBookBtnState(){
    if (!bookBtn) return;
    const disabled = !selectedSeatIds || selectedSeatIds.size === 0;
    bookBtn.disabled = disabled;
    setBtnState?.(disabled ? "idle" : "ready");
  }
  if (bookBtn && !bookBtn.dataset.bound) {
    bookBtn.addEventListener("click", (e) => {
      if (bookBtn.disabled) return;
      bookTicket?.();
    }, false);
    bookBtn.dataset.bound = "1";
  }

  // khởi tạo trạng thái ban đầu
  updateTotalPrice?.();
  updateBookBtnState();
});
