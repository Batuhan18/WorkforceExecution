document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: document.getElementById('email').value,
      password: document.getElementById('password').value
    })
  });
  const body = await res.json();
  if (!res.ok || !body.isSuccess) {
    const el = document.getElementById('error');
    el.textContent = body.error || 'Giriş başarısız.';
    el.classList.remove('d-none');
    return;
  }
  localStorage.setItem('wx_token', body.data.token);
  localStorage.setItem('wx_user', JSON.stringify(body.data));
  const role = body.data.role;
  location.href = role === 'TechOffice' ? '/plan'
                : role === 'HeadOfMaster' ? '/tasks'
                : '/approvals';
});
