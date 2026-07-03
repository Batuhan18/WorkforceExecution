const user = requireAuth(null); // tum roller gorebilir
renderNavbar('/report');

(function init() {
  document.getElementById('listDate').value = todayStr();
  loadReport();
  // PM onayladikca DailyReportUpdated yayinlanir -> rapor kendini canli yeniler
  initSignalR((event) => { if (event === 'DailyReportUpdated' || event === 'WorkItemApproved') loadReport(); });
})();

async function loadReport() {
  const date = document.getElementById('listDate').value;
  const data = await api('/api/reports/daily?date=' + date);
  if (!data) return;

  const k = data.kpis;
  const cards = [
    ['Onaylı Kayıt', k.approvedCount],
    ['Plan Quantity', fmt(k.totalPlannedQuantity)],
    ['Fact Quantity', fmt(k.totalFactQuantity)],
    ['Plan Man-Day', fmt(k.totalPlannedManday)],
    ['Fact Man-Day', fmt(k.totalFactManday)],
    ['Overtime', fmt(k.totalOvertime)],
    ['Verimlilik %', k.overallEfficiency ?? '-']
  ];
  document.getElementById('kpiCards').innerHTML = cards.map(([label, value]) => `
    <div class="col-6 col-md-3 col-xl">
      <div class="card kpi-card p-3 text-center"><small>${label}</small><h3>${value}</h3></div>
    </div>`).join('');

  document.getElementById('rows').innerHTML = data.rows.map(w => `
    <tr>
      <td>${w.locationCode}</td><td class="small">${w.projectCode}</td>
      <td>${w.towCode} / ${w.stowCode} / ${w.sstowCode}</td>
      <td class="small">${w.typeOfCode}</td>
      <td>${fmt(w.plannedQuantity)} ${w.unit}</td><td>${fmt(w.factQuantity)}</td>
      <td>${fmt(w.plannedManday)}</td><td>${fmt(w.factManday)}</td>
      <td>${fmt(w.overtime)}</td><td>${w.efficiency ?? '-'}</td>
      <td class="small">${w.crews.map(c => c.workerTypeName).join(', ') || '-'}</td>
      <td class="small">${w.zzzDetail || '-'}</td>
    </tr>`).join('') || '<tr><td colspan="12" class="text-muted">Bu tarihte onaylanmış kayıt yok.</td></tr>';
}
