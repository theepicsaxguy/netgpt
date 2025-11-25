import axios, { AxiosRequestConfig, AxiosRequestTransformer } from 'axios';
import dayjs from 'dayjs';

const axiosInstance = axios.create();

const appApiUrl = 'http://localhost:5000'; // TODO: Load from runtime.json or env

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
		baseURL: appApiUrl,
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