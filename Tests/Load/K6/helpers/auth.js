import http from 'k6/http';
import { checkLoginResponse } from './checks.js';
import { jsonHeaders } from './headers.js';

/**
 * Provides reusable authentication helpers for k6 load tests.
 *
 * These helpers are intentionally synchronous from the perspective of k6 scripts,
 * since k6 HTTP calls are synchronous within each virtual user iteration.
 */

/**
 * Authenticates against ListMaker.Api and returns the JWT access token.
 *
 * @param {object} environment - The active environment configuration.
 * @returns {string} The JWT access token.
 * @throws {Error} Thrown when authentication fails or the response is invalid.
 */
export function loginToListMaker(environment) {
    const loginUrl = `${environment.listMakerBaseUrl}/api/auth/login`;

    const requestBody = JSON.stringify({
        username: environment.listMakerUsername,
        password: environment.listMakerPassword
    });

    const response = http.post(loginUrl, requestBody, jsonHeaders());

    checkLoginResponse(response, 'ListMaker login');

    if (response.status !== 200) {
        throw new Error(
            `ListMaker login failed. Status: ${response.status}. Body: ${response.body}`
        );
    }

    const payload = response.json();

    if (!payload || !payload.accessToken) {
        throw new Error('ListMaker login response did not contain a valid accessToken.');
    }

    return payload.accessToken;
}

/**
 * Authenticates against ListReader.Api and returns the JWT access token.
 *
 * @param {object} environment - The active environment configuration.
 * @returns {string} The JWT access token.
 * @throws {Error} Thrown when authentication fails or the response is invalid.
 */
export function loginToListReader(environment) {
    const loginUrl = `${environment.listReaderBaseUrl}/api/auth/login`;

    const requestBody = JSON.stringify({
        username: environment.listReaderUsername,
        password: environment.listReaderPassword
    });

    const response = http.post(loginUrl, requestBody, jsonHeaders());

    checkLoginResponse(response, 'ListReader login');

    if (response.status !== 200) {
        throw new Error(
            `ListReader login failed. Status: ${response.status}. Body: ${response.body}`
        );
    }

    const payload = response.json();

    if (!payload || !payload.accessToken) {
        throw new Error('ListReader login response did not contain a valid accessToken.');
    }

    return payload.accessToken;
}
