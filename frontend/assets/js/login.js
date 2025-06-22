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

        // Ẩn nút Admin nếu không phải admin
        if (adminLink) {
          adminLink.style.display = (role === "Admin") ? "list-item" : "none";
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
      <div class="auth-buttons" style="
        display: flex;
        gap: 8px;
        align-items: center;
      ">
        <a href="../../pages/account/register.html" class="auth-btn register-btn" style="
          display: inline-flex;
          align-items: center;
          gap: 8px;
          text-decoration: none;
          padding: 10px 20px;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: white;
          border-radius: 25px;
          font-weight: 500;
          font-size: 14px;
          transition: all 0.3s ease;
          box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
          border: 1px solid rgba(255,255,255,0.2);
          backdrop-filter: blur(10px);
          text-transform: uppercase;
          letter-spacing: 0.5px;
        " onmouseover="this.style.transform='translateY(-2px) scale(1.02)'; this.style.boxShadow='0 8px 25px rgba(102, 126, 234, 0.4)'" 
           onmouseout="this.style.transform='translateY(0) scale(1)'; this.style.boxShadow='0 4px 15px rgba(102, 126, 234, 0.3)'">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M16 7C16 9.20914 14.2091 11 12 11C9.79086 11 8 9.20914 8 7C8 4.79086 9.79086 3 12 3C14.2091 3 16 4.79086 16 7Z" stroke="currentColor" stroke-width="2"/>
            <path d="M12 14C8.13401 14 5 17.134 5 21H19C19 17.134 15.866 14 12 14Z" stroke="currentColor" stroke-width="2"/>
          </svg>
          Đăng ký
        </a>
        
        <a href="../../pages/account/login.html" class="auth-btn login-btn" style="
          display: inline-flex;
          align-items: center;
          gap: 8px;
          text-decoration: none;
          padding: 10px 20px;
          background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
          color: white;
          border-radius: 25px;
          font-weight: 500;
          font-size: 14px;
          transition: all 0.3s ease;
          box-shadow: 0 4px 15px rgba(17, 153, 142, 0.3);
          border: 1px solid rgba(255,255,255,0.2);
          backdrop-filter: blur(10px);
          text-transform: uppercase;
          letter-spacing: 0.5px;
        " onmouseover="this.style.transform='translateY(-2px) scale(1.02)'; this.style.boxShadow='0 8px 25px rgba(17, 153, 142, 0.4)'" 
           onmouseout="this.style.transform='translateY(0) scale(1)'; this.style.boxShadow='0 4px 15px rgba(17, 153, 142, 0.3)'">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15 3H19C20.1046 3 21 3.89543 21 5V19C21 20.1046 20.1046 21 19 21H5C3.89543 21 3 20.1046 3 19V5C3 3.89543 3.89543 3 5 3H9" stroke="currentColor" stroke-width="2"/>
            <path d="M10 17L15 12L10 7" stroke="currentColor" stroke-width="2"/>
            <path d="M15 12H3" stroke="currentColor" stroke-width="2"/>
          </svg>
          Đăng nhập
        </a>
      </div>
    `;
  }
}

function logout() {
  // Thêm hiệu ứng fade out trước khi logout
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

// Thêm CSS animation keyframes vào head
function addAnimationStyles() {
  if (!document.getElementById('auth-styles')) {
    const style = document.createElement('style');
    style.id = 'auth-styles';
    style.textContent = `
      @keyframes fadeInUp {
        from {
          opacity: 0;
          transform: translateY(10px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
      
      .auth-buttons {
        animation: fadeInUp 0.5s ease-out;
      }
      
      .user-profile {
        animation: fadeInUp 0.5s ease-out;
      }
      
      .auth-btn:active {
        transform: translateY(-1px) scale(0.98) !important;
      }
      
      @media (max-width: 768px) {
        .auth-buttons {
          flex-direction: column;
          gap: 6px;
        }
        
        .auth-btn {
          padding: 8px 16px !important;
          font-size: 12px !important;
        }
        
        .user-profile {
          padding: 6px 12px !important;
        }
        
        .user-profile span {
          font-size: 12px !important;
        }
      }
    `;
    document.head.appendChild(style);
  }
}

addAnimationStyles();