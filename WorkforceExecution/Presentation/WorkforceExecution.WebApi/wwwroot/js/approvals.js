const user = requireAuth(['HeadOfMaster', 'SiteChief', 'ProjectManager']);
renderNavbar('/approvals');

(function init() {
  document.getElementById('listDate').value = todayStr();
  loadApprovals();
  initSignalR(() => loadApprovals()); // ApprovalRequested geldiginde kuyruk canli yenilenir
})();

async function loadApprovals() {
  const date = document.getElementById('listDate').value;
  const data = await api('/api/workitems/pending-approvals?date=' + date);
  if (!data) return;
  document.getElementById('rows').innerHTML = data.map(w => {
    const crew = w.crews.map(c => `${c.workerTypeName} ×${c.workerCount}`).join(', ') || '-';
    const history = w.approvals.map(a =>
      `<span class="badge text-bg-${a.isApproved ? 'success' : 'danger'} badge-status me-1">${a.approverRole}</span>`).join('') || '-';
    return `<tr>
      <td>${w.locationCode}</td><td class="small">${w.projectCode}</td>
      <td>${w.towCode} / ${w.stowCode} / <strong>${w.sstowCode}</strong></td>
      <td>${fmt(w.plannedQuantity)} ${w.unit} / ${fmt(w.plannedManday)}</td>
      <td>${fmt(w.factQuantity)} ${w.unit} / ${fmt(w.factManday)}</td>
      <td>${w.efficiency ?? '-'}</td>
      <td class="small">${crew}</td>
      <td class="small">${w.comment || '-'}</td>
      <td>${history}</td>
      <td>
        <button class="btn btn-sm btn-success me-1" onclick="approve(${w.id})">Onayla</button>
        <button class="btn btn-sm btn-outline-danger" onclick="reject(${w.id})">Reddet</button>
      </td>
    </tr>`;
  }).join('') || '<tr><td colspan="10" class="text-muted">Onay bekleyen kayıt yok.</td></tr>';
}

async function approve(id) {
  const data = await api(`/api/workitems/${id}/approve`, { method: 'POST', body: JSON.stringify({}) });
  if (data) { toast('Kayıt onaylandı.', 'success'); loadApprovals(); }
}

async function reject(id) {
  const comment = prompt('Red açıklaması (zorunlu):');
  if (!comment) return;
  const data = await api(`/api/workitems/${id}/reject`, { method: 'POST', body: JSON.stringify({ comment }) });
  if (data) { toast('Kayıt reddedildi.', 'danger'); loadApprovals(); }
}
