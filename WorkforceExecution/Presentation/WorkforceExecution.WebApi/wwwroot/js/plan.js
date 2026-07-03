const user = requireAuth(['TechOffice']);
renderNavbar('/plan');

let lookups = null;
let rowId = 0;

(async function init() {
  // Varsayilan: yarinki plan (T-1 mantigi)
  const tomorrow = new Date(Date.now() + 86400000).toISOString().slice(0, 10);
  document.getElementById('planDate').value = tomorrow;
  document.getElementById('listDate').value = todayStr();

  lookups = await api('/api/lookups');
  addRow();
  loadMine();
  initSignalR(() => loadMine());
})();

function addRow() {
  const id = ++rowId;
  const tr = document.createElement('tr');
  tr.id = 'row-' + id;
  tr.innerHTML = `
    <td><select class="form-select form-select-sm" id="loc-${id}" onchange="onLocationChange(${id})">
      ${lookups.locations.map(l => `<option value="${l.id}">${l.code}</option>`).join('')}</select></td>
    <td><select class="form-select form-select-sm" id="proj-${id}"></select></td>
    <td><select class="form-select form-select-sm" id="tow-${id}" onchange="onTowChange(${id})">
      ${lookups.workCatalog.map((t, i) => `<option value="${i}">${t.code}</option>`).join('')}</select></td>
    <td><select class="form-select form-select-sm" id="stow-${id}" onchange="onStowChange(${id})"></select></td>
    <td><select class="form-select form-select-sm" id="sstow-${id}" onchange="onSstowChange(${id})"></select></td>
    <td><input class="form-control form-control-sm" id="unit-${id}" disabled style="width:70px"></td>
    <td><input type="number" step="0.001" min="0" class="form-control form-control-sm" id="qty-${id}" style="width:110px"></td>
    <td><input type="number" step="0.5" min="0" class="form-control form-control-sm" id="md-${id}" style="width:100px"></td>
    <td><button class="btn btn-sm btn-outline-danger" onclick="document.getElementById('row-${id}').remove()">×</button></td>`;
  document.getElementById('planRows').appendChild(tr);
  onLocationChange(id);
  onTowChange(id);
}

function onLocationChange(id) {
  const locId = Number(document.getElementById('loc-' + id).value);
  const projects = lookups.projects.filter(p => p.locationId === locId);
  document.getElementById('proj-' + id).innerHTML =
    projects.map(p => `<option value="${p.id}">${p.code}</option>`).join('');
}

function onTowChange(id) {
  const tow = lookups.workCatalog[Number(document.getElementById('tow-' + id).value)];
  document.getElementById('stow-' + id).innerHTML =
    tow.subTypes.map((s, i) => `<option value="${i}">${s.code}</option>`).join('');
  onStowChange(id);
}

function onStowChange(id) {
  const tow = lookups.workCatalog[Number(document.getElementById('tow-' + id).value)];
  const stow = tow.subTypes[Number(document.getElementById('stow-' + id).value)];
  document.getElementById('sstow-' + id).innerHTML =
    stow.subSubTypes.map(x => `<option value="${x.id}" data-unit="${x.unit}">${x.code}</option>`).join('');
  onSstowChange(id);
}

function onSstowChange(id) {
  const sel = document.getElementById('sstow-' + id);
  document.getElementById('unit-' + id).value = sel.selectedOptions[0]?.dataset.unit || '-';
}

async function submitPlan() {
  const items = [];
  for (const tr of document.querySelectorAll('#planRows tr')) {
    const id = tr.id.split('-')[1];
    const qty = parseFloat(document.getElementById('qty-' + id).value);
    const md = parseFloat(document.getElementById('md-' + id).value);
    if (isNaN(qty) || isNaN(md)) { toast('Tüm satırlarda Quantity ve Man Day dolu olmalı.', 'warning'); return; }
    items.push({
      locationId: Number(document.getElementById('loc-' + id).value),
      projectId: Number(document.getElementById('proj-' + id).value),
      subSubTypeOfWorkId: Number(document.getElementById('sstow-' + id).value),
      plannedQuantity: qty,
      plannedManday: md
    });
  }
  if (!items.length) { toast('Plan en az bir satır içermeli.', 'warning'); return; }

  const data = await api('/api/dailyplans', {
    method: 'POST',
    body: JSON.stringify({ reportDate: document.getElementById('planDate').value, items })
  });
  if (data) {
    toast(`${data.length} satırlık plan oluşturuldu ve HoM'lara atandı.`, 'success');
    document.getElementById('planRows').innerHTML = '';
    addRow();
    document.getElementById('listDate').value = document.getElementById('planDate').value;
    loadMine();
  }
}

async function loadMine() {
  const date = document.getElementById('listDate').value;
  const data = await api('/api/dailyplans/mine?date=' + date);
  if (!data) return;
  document.getElementById('mineRows').innerHTML = data.map(w => `
    <tr>
      <td>${w.locationCode}</td><td class="small">${w.projectCode}</td>
      <td>${w.towCode} / ${w.stowCode} / ${w.sstowCode}</td>
      <td>${fmt(w.plannedQuantity)} ${w.unit}</td><td>${fmt(w.plannedManday)}</td>
      <td>${fmt(w.factQuantity)}</td><td>${fmt(w.factManday)}</td>
      <td class="small">${w.assignedHomEmail}</td>
      <td>${statusBadge(w.status)}</td>
    </tr>`).join('') || '<tr><td colspan="9" class="text-muted">Kayıt yok.</td></tr>';
}
