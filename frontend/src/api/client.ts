import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';

const AXIOS_INSTANCE = axios.create({
  baseURL: (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000', // Configurable API URL
});

AXIOS_INSTANCE.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: unknown) => {
    console.error('API call failed:', error);
    return Promise.reject(error);
  }
);

export const customInstance = <T>(
  config: AxiosRequestConfig,
  options?: AxiosRequestConfig
): Promise<T> => {
  const source = axios.CancelToken.source();
  const promise = AXIOS_INSTANCE({
    ...config,
    ...options,
    cancelToken: source.token,
  }).then(({ data }: AxiosResponse<T>) => data);

  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-ignore
  promise.cancel = () => {
    source.cancel('Query was cancelled');
  };

  return promise;
};