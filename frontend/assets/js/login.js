// ========== Parse & helpers ==========
function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    while (base64.length % 4) base64 += '='; // padding
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (e) {
    console.error("Lỗi parseJwt:", e);
    return null;
  }
}

function getClaim(obj, keys) {
  if (!obj) return null;
  for (const k of keys) {
    if (Object.prototype.hasOwnProperty.call(obj, k) && obj[k] != null) {
      return obj[k];
    }
  }
  return null;
}

function getUserIdFromToken(token) {
  const p = parseJwt(token);
  return getClaim(p, [
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", // MS
    "sub",           // chuẩn JWT
    "UserId",        // custom
    "unique_name",   // đôi khi BE set
    "name"
  ]);
}

function getRoleFromToken(token) {
  const p = parseJwt(token);
  let role = getClaim(p, [
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
    "role",
    "Role"
  ]);
  if (Array.isArray(role)) role = role[0] || "";
  return role || "";
}

function getFullNameFromToken(token) {
  const p = parseJwt(token);
  return getClaim(p, ["FullName", "name", "unique_name", "given_name"]) || "Người dùng";
}

// ========== Header UI ==========
function updateAuthButtons() {
  const authContainer = document.getElementById("authButtons");
  const adminLink = document.getElementById("adminControlLink");
  if (!authContainer) {
    console.warn("Không tìm thấy #authButtons trong header.");
    return;
  }

  const token = localStorage.getItem("token");
  if (token) {
    const fullName = getFullNameFromToken(token);
    const role = getRoleFromToken(token);

    if (adminLink) {
      adminLink.style.display = (role === "Admin" || role === "Employee") ? "list-item" : "none";
    }

    authContainer.innerHTML = `
      <div class="user-profile" style="display:flex;align-items:center;gap:12px;padding:8px 16px;
        background:linear-gradient(135deg,rgba(255,255,255,.1),rgba(255,255,255,.05));
        border-radius:25px;backdrop-filter:blur(10px);border:1px solid rgba(255,255,255,.2);">
        <div class="avatar" style="width:32px;height:32px;border-radius:50%;
          display:flex;align-items:center;justify-content:center;background:linear-gradient(45deg,#667eea,#764ba2);color:#fff;font-weight:600;">
          ${String(fullName).charAt(0).toUpperCase()}
        </div>
        <span style="color:#fff">Xin chào, <strong>${fullName}</strong></span>
        <button onclick="logout()" style="padding:6px 14px;border:none;border-radius:20px;background:#ee5a52;color:#fff;cursor:pointer;">
          Đăng xuất
        </button>
      </div>`;
  } else {
    authContainer.innerHTML = `
      <div class="auth-buttons" style="display:flex;gap:8px;align-items:center;">
        <a href="../../pages/account/register.html" class="auth-btn register-btn"
           style="padding:10px 20px;background:linear-gradient(135deg,#667eea,#764ba2);color:#fff;border-radius:25px;">Đăng ký</a>
        <a href="../../pages/account/login.html" class="auth-btn login-btn"
           style="padding:10px 20px;background:linear-gradient(135deg,#11998e,#38ef7d);color:#fff;border-radius:25px;">Đăng nhập</a>
      </div>`;
  }
}

// Ví dụ dùng ở booking.js khi kết nối hub:
function getCurrentUserId() {
  const t = localStorage.getItem("token");
  return t ? getUserIdFromToken(t) : null;
}


function logout() {
  const authContainer = document.getElementById("authButtons");
  if (authContainer) {
    authContainer.style.transition = 'opacity 0.3s ease';
    authContainer.style.opacity = '0.5';
  }

  setTimeout(() => {
    localStorage.removeItem("token");
    window.location.href = "../Phim/index.html";
  }, 300);
}

function addAnimationStyles() {
  if (!document.getElementById('auth-styles')) {
    const style = document.createElement('style');
    style.id = 'auth-styles';
    style.textContent = `
      @keyframes fadeInUp {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
      }
      .auth-buttons, .user-profile { animation: fadeInUp 0.5s ease-out; }
    `;
    document.head.appendChild(style);
  }
}

addAnimationStyles();
updateAuthButtons();

