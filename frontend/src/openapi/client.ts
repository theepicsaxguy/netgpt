import axios, { AxiosRequestConfig, AxiosRequestTransformer } from 'axios';
import dayjs from 'dayjs';

const axiosInstance = axios.create();

// Interceptor: if a request would go to an absolute localhost address from a
// remote origin (e.g., dev-tunnel), rewrite it into a relative path so the
// browser issues the request to the current origin (Vite dev server) which
// can proxy it to the real backend. This prevents the browser from attempting
// to call the backend's localhost address where it won't exist.
axiosInstance.interceptors.request.use((config) => {
	try {
		const url = (config.url ?? '').toString();

		// If the configured URL is absolute and points to localhost or 127.0.0.1,
		// convert it to a relative path (strip protocol+host+port).
		const localhostRegex = /^https?:\/\/(localhost|127\.0\.0\.1)(:\d+)?(\/.*)?$/i;
		const match = url.match(localhostRegex);
		if (match) {
			const path = match[3] ?? '/';
			// eslint-disable-next-line no-console
			console.debug('[openapi/client] Rewriting absolute localhost URL to relative path:', url, '->', path);
			config.url = path;
			// Ensure axios uses relative base (current origin)
			config.baseURL = undefined;
			return config;
		}

		// If baseURL itself points to localhost, clear it so the request goes
		// to the current origin instead.
		if (typeof config.baseURL === 'string' && /localhost|127\.0\.0\.1/.test(config.baseURL)) {
			// eslint-disable-next-line no-console
			console.debug('[openapi/client] Clearing baseURL because it points to localhost:', config.baseURL);
			config.baseURL = undefined;
		}
	} catch (e) {
		// ignore and continue with original config
	}

	return config;
});

// Load runtime config (served from /runtime.json in the public folder) synchronously
// This ensures the API base URL is available during module initialization
function loadRuntimeConfigSync() {
	// If an app has already populated a global runtime, use it
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	if ((globalThis as any).__RUNTIME__) return (globalThis as any).__RUNTIME__;

	try {
		// Use a synchronous XHR to ensure the config is available during module load.
		const xhr = new XMLHttpRequest();
		xhr.open('GET', '/runtime.json', false);
		xhr.send(null);
		if (xhr.status >= 200 && xhr.status < 300 && xhr.responseText) {
			const parsed = JSON.parse(xhr.responseText);
			// @ts-ignore
			globalThis.__RUNTIME__ = parsed;
			return parsed;
		}
	} catch (e) {
		// ignore and fallback
	}

	// Fallback default (keeps previous behavior)
	return { api: { app: 'http://localhost:5000' } };
}

const runtime = loadRuntimeConfigSync();
let appApiUrl = runtime?.api?.app ?? 'http://localhost:5000';

// If we're running in the browser on localhost during development, prefer a relative
// base URL so Vite's dev server proxy can forward requests to the backend.
try {
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	if (typeof window !== 'undefined' && window?.location) {
		const host = window.location.hostname;
		// If runtime points to localhost, prefer relative paths in two cases:
		// 1) We're running locally (host is localhost) -> use relative so Vite proxy can forward
		// 2) We're running under a dev tunnel / remote hostname (host is NOT localhost) ->
		//    the browser's 'localhost' is not the backend; use relative so requests go to the
		//    current origin (dev tunnel) which maps back to the Vite dev server.
		// If the runtime backend points at localhost/127.0.0.1, prefer relative
		// requests so the dev server (or a dev-tunnel) can proxy them correctly.
		if (typeof appApiUrl === 'string' && /localhost|127\.0\.0\.1/.test(appApiUrl)) {
			appApiUrl = '';
		}
	}
} catch (e) {
	// ignore - non-browser environments
}

// Final base to pass to axios. If empty string, use undefined so axios will make
// relative requests against the current origin (this avoids accidental absolute
// URL resolution behavior in some environments).
const finalBaseUrl = appApiUrl === '' ? undefined : appApiUrl;

// Determine baseURL to use at request time. If we're in the browser and the runtime
// API points at localhost, we MUST use a relative base (undefined) so that the
// dev server or dev-tunnel proxies requests correctly. Some environments may
// require overriding earlier settings, so compute baseURLUsed per-request.
function computeBaseUrlForRequest() {
	try {
		// In-browser and runtime points to local backend -> use relative
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		if (typeof window !== 'undefined' && runtime && typeof runtime.api?.app === 'string') {
			if (/localhost|127\.0\.0\.1/.test(runtime.api.app)) {
				return undefined;
			}
		}
	} catch (e) {
		// ignore
	}

	return finalBaseUrl;
}

// Debug logging to help diagnose why requests may still go to an absolute host.
try {
	// eslint-disable-next-line no-console
	console.debug('[openapi/client] runtime=', runtime, 'appApiUrl=', appApiUrl, 'finalBaseUrl=', finalBaseUrl, 'location=', typeof window !== 'undefined' ? window.location.href : undefined);
} catch (e) {
	// ignore logging failures
}

type TTransformable = Date | unknown[] | Record<string, unknown> | unknown;

// Add a second `options` argument here if you want to pass extra options to each generated query
export const customInstance = <T>(
	config: AxiosRequestConfig,
	options?: AxiosRequestConfig,
): Promise<T> => {
	const source = axios.CancelToken.source();
	const promise = axiosInstance({
		...config,
		...options,
		transformRequest: [
			dateTransformer,
			...((axios.defaults.transformRequest as
				| AxiosRequestTransformer[]
				| undefined) || []),
		],
		baseURL: computeBaseUrlForRequest(),
		cancelToken: source.token,
	}).then(({ data }) => data);

	// @ts-expect-error Required for orval
	promise.cancel = () => {
		source.cancel('Query was cancelled');
	};

	config.transformRequest = [];

	return promise;
};

const dateTransformer = (data: TTransformable): TTransformable => {
	if (data instanceof Date) {
		// Format Date instances
		return dayjs(data).format('YYYY-MM-DDTHH:mm:ss');
	}

	if (Array.isArray(data)) {
		// Recursively transform array elements
		return data.map((val) => dateTransformer(val));
	}

	if (typeof data === 'object' && data !== null) {
		// Recursively transform object properties
		return Object.fromEntries(
			Object.entries(data).map(([key, val]) => [
				key,
				dateTransformer(val),
			]),
		);
	}

	// Return data unchanged if it's neither a Date, array, nor object
	return data;
};