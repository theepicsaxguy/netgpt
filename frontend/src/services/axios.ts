import axios from 'axios';
import { getAccessToken, refresh } from './auth';

const instance = axios.create();

let isRefreshing = false;
let failedQueue: Array<{ resolve: (val?: any) => void; reject: (err: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((p) => {
    if (error) p.reject(error);
    else p.resolve(token);
  });
  failedQueue = [];
};

instance.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers = config.headers ?? {};
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

instance.interceptors.response.use(
  (res) => res,
  async (err) => {
    const originalRequest = err.config;
    if (err.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise(function (resolve, reject) {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers['Authorization'] = 'Bearer ' + token;
            return axios(originalRequest);
          })
          .catch((e) => Promise.reject(e));
      }

      originalRequest._retry = true;
      isRefreshing = true;
      try {
        const newToken = await refresh();
        isRefreshing = false;
        processQueue(null, newToken);
        if (newToken) {
          originalRequest.headers['Authorization'] = 'Bearer ' + newToken;
          return axios(originalRequest);
        }
      } catch (e) {
        isRefreshing = false;
        processQueue(e, null);
        return Promise.reject(e);
      }
    }
    return Promise.reject(err);
  }
);

export default instance;
import axios from 'axios';
import { getAccessToken, refresh, setAccessToken, clear } from './auth';

const api = axios.create({ baseURL: '/api' });

api.interceptors.request.use((cfg) => {
  const token = getAccessToken();
  if (token) cfg.headers = { ...(cfg.headers || {}), Authorization: `Bearer ${token}` };
  return cfg;
});

let isRefreshing = false;
let failedQueue: Array<{ resolve: (v?: any) => void; reject: (err: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(p => {
    if (error) p.reject(error);
    else p.resolve(token);
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (r) => r,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise(function (resolve, reject) {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers['Authorization'] = 'Bearer ' + token;
            return api(originalRequest);
          })
          .catch(err => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;
      const newToken = await refresh();
      if (newToken) {
        setAccessToken(newToken, Date.now() + 15 * 60 * 1000);
        processQueue(null, newToken);
        originalRequest.headers['Authorization'] = 'Bearer ' + newToken;
        isRefreshing = false;
        return api(originalRequest);
      } else {
        processQueue(new Error('Refresh failed'), null);
        clear();
        isRefreshing = false;
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
