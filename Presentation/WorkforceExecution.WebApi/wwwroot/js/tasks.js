const user = requireAuth(['HeadOfMaster']);
renderNavbar('/tasks');

let lookups = null;
let currentItemId = null;
let crewRowId = 0;

(async function init() {
  document.getElementById('listDate').value = todayStr();
  lookups = await api('/api/lookups');
  loadTasks();
  initSignalR(() => loadTasks()); // PlanAssigned vb. geldiginde canli yenile
})();

async function loadTasks() {
  const date = document.getElementById('listDate').value;
  const data = await api('/api/workitems/assigned?date=' + date);
  if (!data) return;
  document.getElementById('taskRows').innerHTML = data.map(w => {
    const crew = w.crews.map(c => `${c.workerTypeName} ×${c.workerCount}`).join(', ') || '-';
    const fact = w.factQuantity !== null ? `${fmt(w.factQuantity)} ${w.unit} / ${fmt(w.factManday)} MD` : '-';
    let actions = '';
    if (w.status === 'Planned')
      actions = `<button class="btn btn-sm btn-primary" onclick="openCrewModal(${w.id})">Crew Oluştur</button>`;
    else if (w.status === 'InProgress' || w.status === 'Rejected')
      actions = `<button class="btn btn-sm btn-success" onclick="openFactModal(${w.id})">Gün Sonu Gir</button>`;
    else if (w.status === 'PendingHomApproval')
      actions = `<button class="btn btn-sm btn-success me-1" onclick="approve(${w.id})">Onayla</button>
                 <button class="btn btn-sm btn-outline-danger" onclick="reject(${w.id})">Reddet</button>`;
    return `<tr>
      <td>${w.locationCode}</td><td class="small">${w.projectCode}</td>
      <td>${w.towCode} / ${w.stowCode} / <strong>${w.sstowCode}</strong><br>
          <span class="text-muted small">${w.typeOfCode}</span></td>
      <td>${fmt(w.plannedQuantity)} ${w.unit}</td><td>${fmt(w.plannedManday)}</td>
      <td class="small">${crew}</td><td class="small">${fact}</td>
      <td>${statusBadge(w.status)}${w.comment ? `<br><span class="text-muted small">${w.comment}</span>` : ''}</td>
      <td>${actions}</td>
    </tr>`;
  }).join('') || '<tr><td colspan="9" class="text-muted">Bu tarihte atanmış iş yok.</td></tr>';
}

// ---- Crew ----
function openCrewModal(id) {
  currentItemId = id;
  document.getElementById('crewRows').innerHTML = '';
  addCrewRow();
  new bootstrap.Modal('#crewModal').show();
}

function addCrewRow() {
  const id = ++crewRowId;
  const tr = document.createElement('tr');
  tr.id = 'crew-' + id;
  tr.innerHTML = `
    <td><select class="form-select form-select-sm" id="wt-${id}">
      ${lookups.workerTypes.map(w => `<option value="${w.id}">${w.name}</option>`).join('')}</select></td>
    <td style="width:110px"><input type="number" min="1" value="1" class="form-control form-control-sm" id="wc-${id}"></td>
    <td style="width:40px"><button class="btn btn-sm btn-outline-danger" onclick="document.getElementById('crew-${id}').remove()">×</button></td>`;
  document.getElementById('crewRows').appendChild(tr);
}

async function saveCrew() {
  const crews = [...document.querySelectorAll('#crewRows tr')].map(tr => {
    const id = tr.id.split('-')[1];
    return {
      workerTypeId: Number(document.getElementById('wt-' + id).value),
      workerCount: Number(document.getElementById('wc-' + id).value)
    };
  });
  const data = await api(`/api/workitems/${currentItemId}/crew`, { method: 'POST', body: JSON.stringify({ crews }) });
  if (data) {
    bootstrap.Modal.getInstance(document.getElementById('crewModal')).hide();
    toast('Crew oluşturuldu, iş alındı.', 'success');
    loadTasks();
  }
}

// ---- Fact ----
function openFactModal(id) {
  currentItemId = id;
  ['factQty', 'factMd', 'comment', 'zzzDetail'].forEach(x => document.getElementById(x).value = '');
  document.getElementById('overtime').value = '0';
  new bootstrap.Modal('#factModal').show();
}

async function saveFact() {
  const data = await api(`/api/workitems/${currentItemId}/fact`, {
    method: 'POST',
    body: JSON.stringify({
      factQuantity: parseFloat(document.getElementById('factQty').value) || 0,
      factManday: parseFloat(document.getElementById('factMd').value) || 0,
      overtime: parseFloat(document.getElementById('overtime').value) || 0,
      comment: document.getElementById('comment').value || null,
      zzzDetail: document.getElementById('zzzDetail').value || null
    })
  });
  if (data) {
    bootstrap.Modal.getInstance(document.getElementById('factModal')).hide();
    toast('Gün sonu verisi kaydedildi, onayınıza düştü.', 'success');
    loadTasks();
  }
}

// ---- HoM onayi (zincirin ilk halkasi) ----
async function approve(id) {
  const data = await api(`/api/workitems/${id}/approve`, { method: 'POST', body: JSON.stringify({}) });
  if (data) { toast("Onaylandı, Site Chief'e gönderildi.", 'success'); loadTasks(); }
}

async function reject(id) {
  const comment = prompt('Red açıklaması (zorunlu):');
  if (!comment) return;
  const data = await api(`/api/workitems/${id}/reject`, { method: 'POST', body: JSON.stringify({ comment }) });
  if (data) { toast('Kayıt reddedildi.', 'danger'); loadTasks(); }
}
