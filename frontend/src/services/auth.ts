let accessToken: string | null = null;
let expiresAt: number | null = null;

export function setAccessToken(token: string, expires: number) {
  accessToken = token;
  expiresAt = expires;
}

export function getAccessToken() {
  return accessToken;
}

export async function refresh(): Promise<string | null> {
  try {
    const resp = await fetch('/api/auth/refresh', { method: 'POST', credentials: 'include' });
    if (!resp.ok) return null;
    const json = await resp.json();
    setAccessToken(json.accessToken, new Date(json.expiresAt).getTime());
    return json.accessToken;
  } catch (e) {
    return null;
  }
}

export async function login(username: string, password: string) {
  const resp = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
    credentials: 'include'
  });
  if (!resp.ok) throw new Error('Login failed');
  const json = await resp.json();
  setAccessToken(json.accessToken, new Date(json.expiresAt).getTime());
  return json;
}

export function clear() {
  accessToken = null;
  expiresAt = null;
}
let accessToken: string | null = null;
let expiresAt: number | null = null;

export function setAccessToken(token: string, expires: number) {
  accessToken = token;
  expiresAt = expires;
}

export function getAccessToken(): string | null {
  return accessToken;
}

export async function refresh(): Promise<string | null> {
  try {
    const resp = await fetch('/api/auth/refresh', { method: 'POST', credentials: 'include' });
    if (!resp.ok) return null;
    const data = await resp.json();
    setAccessToken(data.accessToken, Date.now() + 15 * 60 * 1000);
    return data.accessToken;
  } catch {
    return null;
  }
}

export function clear() {
  accessToken = null;
  expiresAt = null;
}
