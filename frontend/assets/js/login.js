function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = decodeURIComponent(atob(base64Url).split('').map(c =>
      '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
    return JSON.parse(base64);
  } catch (e) {
    return null;
  }
}

function updateAuthButtons() {
  const authContainer = document.getElementById("authButtons");
  const adminLink = document.getElementById("adminControlLink");

  if (!authContainer) {
    console.warn("Không tìm thấy #authButtons trong header.");
    return;
  }

  const token = localStorage.getItem("token");

  if (token) {
    const decoded = parseJwt(token);
    const fullName = decoded?.FullName || decoded?.name || "Người dùng";
    const role = decoded?.Role || decoded?.role || "";

    // ✅ Hiện Admin Control nếu là Admin hoặc Employee
    if (adminLink) {
      adminLink.style.display = (role === "Admin" || role === "Employee") ? "list-item" : "none";
    }

    authContainer.innerHTML = `
      <div class="user-profile" style="
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 8px 16px;
        background: linear-gradient(135deg, rgba(255,255,255,0.1), rgba(255,255,255,0.05));
        border-radius: 25px;
        backdrop-filter: blur(10px);
        border: 1px solid rgba(255,255,255,0.2);
        box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        transition: all 0.3s ease;
      " onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 6px 20px rgba(0,0,0,0.15)'" 
         onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 15px rgba(0,0,0,0.1)'">
        <div class="avatar" style="
          width: 32px;
          height: 32px;
          background: linear-gradient(45deg, #667eea 0%, #764ba2 100%);
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-weight: bold;
          color: white;
          font-size: 14px;
          box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        ">${fullName.charAt(0).toUpperCase()}</div>

        <span style="
          color: white;
          font-weight: 500;
          font-size: 14px;
          text-shadow: 0 1px 2px rgba(0,0,0,0.3);
        ">Xin chào, <strong>${fullName}</strong></span>

        <button onclick="logout()" style="
          padding: 6px 14px;
          background: linear-gradient(135deg, #ff6b6b, #ee5a52);
          color: white;
          border: none;
          border-radius: 20px;
          font-size: 12px;
          font-weight: 500;
          cursor: pointer;
          transition: all 0.3s ease;
          box-shadow: 0 2px 8px rgba(238, 90, 82, 0.3);
          text-transform: uppercase;
          letter-spacing: 0.5px;
        " onmouseover="this.style.transform='scale(1.05)'; this.style.boxShadow='0 4px 15px rgba(238, 90, 82, 0.4)'" 
           onmouseout="this.style.transform='scale(1)'; this.style.boxShadow='0 2px 8px rgba(238, 90, 82, 0.3)'">
          Đăng xuất
        </button>
      </div>
    `;
  } else {
    authContainer.innerHTML = `
      <div class="auth-buttons" style="display: flex; gap: 8px; align-items: center;">
        <a href="../../pages/account/register.html" class="auth-btn register-btn" style="
          padding: 10px 20px;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: white;
          border-radius: 25px;
          font-weight: 500;
          font-size: 14px;
          text-transform: uppercase;
        ">Đăng ký</a>
        <a href="../../pages/account/login.html" class="auth-btn login-btn" style="
          padding: 10px 20px;
          background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
          color: white;
          border-radius: 25px;
          font-weight: 500;
          font-size: 14px;
          text-transform: uppercase;
        ">Đăng nhập</a>
      </div>
    `;
  }
}

function logout() {
  const authContainer = document.getElementById("authButtons");
  if (authContainer) {
    authContainer.style.transition = 'opacity 0.3s ease';
    authContainer.style.opacity = '0.5';
  }

  setTimeout(() => {
    localStorage.removeItem("token");
    window.location.href = "../../index.html";
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
