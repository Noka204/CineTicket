/* ========== JWT parse & helpers (giữ nguyên) ========== */
function parseJwt(token){
  try{
    const base64Url = token.split('.')[1];
    if(!base64Url) return null;
    let base64 = base64Url.replace(/-/g,'+').replace(/_/g,'/');
    while(base64.length % 4) base64 += '=';
    const jsonPayload = decodeURIComponent(
      atob(base64).split('').map(c=>'%' + ('00'+c.charCodeAt(0).toString(16)).slice(-2)).join('')
    );
    return JSON.parse(jsonPayload);
  }catch(e){ console.error('Lỗi parseJwt:', e); return null; }
}
function getClaim(obj, keys){
  if(!obj) return null;
  for(const k of keys){ if(Object.prototype.hasOwnProperty.call(obj,k) && obj[k]!=null) return obj[k]; }
  return null;
}
function getUserIdFromToken(token){
  const p = parseJwt(token);
  return getClaim(p,[
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    'sub','UserId','unique_name','name'
  ]);
}
function getRoleFromToken(token){
  const p = parseJwt(token);
  let role = getClaim(p,[
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role','role','Role'
  ]);
  if(Array.isArray(role)) role = role[0] || '';
  return role || '';
}
function getFullNameFromToken(token){
  const p = parseJwt(token);
  return getClaim(p,['FullName','name','unique_name','given_name']) || 'Người dùng';
}

/* ========== Link trang cá nhân ========== */
const PROFILE_URL = '../../pages/account/profile.html';

/* ========== Header UI (greeting KHÔNG trong button) ========== */
function updateAuthButtons(){
  const authContainer = document.getElementById('authButtons');
  const adminLink     = document.getElementById('adminControlLink');
  if(!authContainer){ console.warn('Không tìm thấy #authButtons'); return; }

  const token = localStorage.getItem('token');
  if(token){
    const fullName = getFullNameFromToken(token);
    const role     = getRoleFromToken(token);
    const initial  = String(fullName).charAt(0).toUpperCase();

    if(adminLink){
      adminLink.style.display = (role === 'Admin' || role === 'Employee') ? 'list-item' : 'none';
    }

    authContainer.innerHTML = `
      <div class="user-profile" style="display:flex;align-items:center;gap:10px;">
        <div class="avatar" style="
            width:28px;height:28px;border-radius:50%;
            display:flex;align-items:center;justify-content:center;
            background:linear-gradient(45deg,#667eea,#764ba2);color:#fff;
            font-weight:700;font-size:12px;">${initial}</div>

        <!-- Greeting là LINK phẳng, không có nền/viền -->
        <a href="${PROFILE_URL}" class="hello-link" title="Trang cá nhân" style="
            display:inline-block;padding:0;margin:0;
            background:transparent;border:0;border-radius:0;box-shadow:none;
            color:#fff;text-decoration:none;font-size:14px;white-space:nowrap;">
          Xin chào, <strong>${fullName}</strong>
        </a>

        <!-- Giữ nút đăng xuất như cũ -->
        <button onclick="logout()" style="
            padding:6px 14px;border:none;border-radius:20px;
            background:#ee5a52;color:#fff;cursor:pointer;">Đăng xuất</button>
      </div>
    `;
  }else{
    authContainer.innerHTML = `
      <div class="auth-buttons" style="display:flex;gap:8px;align-items:center;">
        <a href="../../pages/account/register.html" class="auth-btn register-btn"
           style="padding:10px 20px;background:linear-gradient(135deg,#667eea,#764ba2);color:#fff;border-radius:25px;">Đăng ký</a>
        <a href="../../pages/account/login.html" class="auth-btn login-btn"
           style="padding:10px 20px;background:linear-gradient(135deg,#11998e,#38ef7d);color:#fff;border-radius:25px;">Đăng nhập</a>
      </div>`;
    if(adminLink) adminLink.style.display = 'none';
  }
}

/* ========== Dùng nơi khác nếu cần ========== */
function getCurrentUserId(){
  const t = localStorage.getItem('token');
  return t ? getUserIdFromToken(t) : null;
}
function logout(){
  const authContainer = document.getElementById('authButtons');
  if(authContainer){
    authContainer.style.transition = 'opacity .3s ease';
    authContainer.style.opacity = '0.5';
  }
  setTimeout(()=>{ localStorage.removeItem('token'); window.location.href = '../Phim/index.html'; },300);
}

/* ========== Hiệu ứng nhỏ + ép greeting không bị CSS global override ========== */
(function addAuthStyles(){
  if (document.getElementById('auth-styles')) return;
  const style = document.createElement('style');
  style.id = 'auth-styles';
  style.textContent = `
    @keyframes fadeInUp { from{opacity:0;transform:translateY(10px)} to{opacity:1;transform:translateY(0)} }
    .auth-buttons,.user-profile{ animation:fadeInUp .5s ease-out; }

    /* canh giữa avatar – text – nút */
    .user-profile{
      display:flex; align-items:center; gap:10px; line-height:1;
    }

    /* greeting là link phẳng, hạ xuống 1–2px */
    a.hello-link{
      display:inline-flex !important;
      align-items:center !important;
      line-height:1 !important;
      padding:0 !important; margin:0 !important;
      background:transparent !important; border:0 !important; border-radius:0 !important; box-shadow:none !important;
      transform: translateY(1px); /* tăng lên 2px nếu vẫn thấy cao */
      color:#fff; text-decoration:none;
    }
    a.hello-link:hover{ text-decoration:underline; }
  `;
  document.head.appendChild(style);
})();

updateAuthButtons();
