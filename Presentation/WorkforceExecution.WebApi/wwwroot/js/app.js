// ---- Auth yardimcilari ----
const wx = {
  token: () => localStorage.getItem('wx_token'),
  user: () => JSON.parse(localStorage.getItem('wx_user') || 'null'),
  logout: () => { localStorage.clear(); location.href = '/'; }
};

function requireAuth(allowedRoles) {
  const user = wx.user();
  if (!user || !wx.token()) { location.href = '/'; return null; }
  if (allowedRoles && !allowedRoles.includes(user.role)) { location.href = '/report'; return null; }
  return user;
}

// ---- API cagri sarmalayicisi: JWT header + hata yonetimi ----
async function api(path, options = {}) {
  const res = await fetch(path, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + wx.token(),
      ...(options.headers || {})
    }
  });
  if (res.status === 401) { wx.logout(); return null; }
  const body = await res.json();
  if (!body.isSuccess) { toast(body.error || 'İşlem başarısız.', 'danger'); return null; }
  return body.data;
}

// ---- Navbar ----
function renderNavbar(active) {
  const user = wx.user();
  const links = [];
  if (user.role === 'TechOffice') links.push(['/plan', 'Günlük Plan']);
  if (user.role === 'HeadOfMaster') links.push(['/tasks', 'Görevlerim']);
  if (['HeadOfMaster', 'SiteChief', 'ProjectManager'].includes(user.role)) links.push(['/approvals', 'Onay Kuyruğu']);
  links.push(['/report', 'Daily Report']);

  document.getElementById('navbar').innerHTML = `
    <nav class="navbar navbar-expand navbar-wx px-3 mb-4">
      <span class="navbar-brand">Workforce Execution</span>
      <ul class="navbar-nav me-auto">
        ${links.map(([href, label]) =>
          `<li class="nav-item"><a class="nav-link ${active === href ? 'active' : ''}" href="${href}">${label}</a></li>`).join('')}
      </ul>
      <span class="text-white-50 small me-3">${user.fullName} · ${roleLabel(user.role)}</span>
      <button class="btn btn-sm btn-outline-light" onclick="wx.logout()">Çıkış</button>
    </nav>`;
}

function roleLabel(role) {
  return { TechOffice: 'Teknik Ofis', HeadOfMaster: 'Head of Master',
           SiteChief: 'Site Chief', ProjectManager: 'Project Manager' }[role] || role;
}

// ---- Durum rozetleri ----
function statusBadge(status) {
  const map = {
    Planned:            ['secondary', 'Planlandı'],
    InProgress:         ['info', 'Devam Ediyor'],
    PendingHomApproval: ['warning', 'HoM Onayı Bekliyor'],
    PendingSiteChief:   ['warning', 'Site Chief Bekliyor'],
    PendingPm:          ['warning', 'PM Bekliyor'],
    Approved:           ['success', 'Onaylandı'],
    Rejected:           ['danger', 'Reddedildi']
  };
  const [color, label] = map[status] || ['secondary', status];
  return `<span class="badge text-bg-${color} badge-status">${label}</span>`;
}

// ---- Toast bildirimleri ----
function toast(message, type = 'primary') {
  let container = document.querySelector('.toast-container');
  if (!container) {
    container = document.createElement('div');
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    document.body.appendChild(container);
  }
  const el = document.createElement('div');
  el.className = `toast align-items-center text-bg-${type} border-0`;
  el.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div>
    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>`;
  container.appendChild(el);
  new bootstrap.Toast(el, { delay: 5000 }).show();
}

// ---- SignalR: JWT ile hub baglantisi + olay -> toast/refresh ----
async function initSignalR(onEvent) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications', { accessTokenFactory: () => wx.token() })
    .withAutomaticReconnect()
    .build();

  const events = {
    PlanAssigned:      d => `Yeni iş atandı: ${d.locationCode} / ${d.sstowCode}`,
    CrewAssigned:      d => `Crew oluşturuldu: ${d.locationCode} / ${d.sstowCode}`,
    FactSubmitted:     d => `Gün sonu verisi girildi: ${d.locationCode} / ${d.sstowCode}`,
    ApprovalRequested: d => `Onay bekleyen kayıt: ${d.locationCode} / ${d.sstowCode}`,
    WorkItemApproved:  d => `Kayıt onaylandı: ${d.locationCode} / ${d.sstowCode}`,
    WorkItemRejected:  d => `Kayıt reddedildi: ${d.locationCode} / ${d.sstowCode}`,
    DailyReportUpdated: () => `Daily Report güncellendi`
  };

  for (const [name, msgFn] of Object.entries(events)) {
    connection.on(name, payload => {
      toast(msgFn(payload), name === 'WorkItemRejected' ? 'danger' : 'primary');
      if (onEvent) onEvent(name, payload);
    });
  }

  try { await connection.start(); console.log('SignalR bağlı'); }
  catch (err) { console.error('SignalR bağlantı hatası', err); }
  return connection;
}

const fmt = (v) => v === null || v === undefined ? '-' : Number(v).toLocaleString('tr-TR');
const todayStr = () => new Date().toISOString().slice(0, 10);
