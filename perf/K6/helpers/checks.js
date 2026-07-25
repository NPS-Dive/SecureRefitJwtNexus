import { check } from 'k6';

/**
 * Provides reusable validation helpers for k6 HTTP responses.
 */

/**
 * Verifies a login response.
 *
 * Expected behavior:
 * - HTTP 200 OK
 * - JSON body exists
 * - accessToken is present and non-empty
 *
 * @param {object} response - The k6 HTTP response object.
 * @param {string} label - A human-readable label to distinguish checks in output.
 * @returns {boolean} True when all checks pass; otherwise false.
 */
export function checkLoginResponse(response, label) {
    return check(response, {
        [`${label} - status is 200`]: (r) => r.status === 200,
        [`${label} - body is not empty`]: (r) => !!r.body,
        [`${label} - accessToken exists`]: (r) => {
            const payload = safeJsonParse(r);
            return !!payload && typeof payload.accessToken === 'string' && payload.accessToken.length > 0;
        }
    });
}

/**
 * Verifies a people-list response returned by the generated-list endpoint.
 *
 * Expected behavior:
 * - HTTP 200 OK
 * - JSON body exists
 * - payload is an array
 * - payload contains at least one person
 *
 * @param {object} response - The k6 HTTP response object.
 * @param {string} label - A human-readable label to distinguish checks in output.
 * @returns {boolean} True when all checks pass; otherwise false.
 */
export function checkPeopleListResponse(response, label) {
    return check(response, {
        [`${label} - status is 200`]: (r) => r.status === 200,
        [`${label} - body is not empty`]: (r) => !!r.body,
        [`${label} - payload is array`]: (r) => {
            const payload = safeJsonParse(r);
            return Array.isArray(payload);
        },
        [`${label} - payload has items`]: (r) => {
            const payload = safeJsonParse(r);
            return Array.isArray(payload) && payload.length > 0;
        },
        [`${label} - first item has required fields`]: (r) => {
            const payload = safeJsonParse(r);

            if (!Array.isArray(payload) || payload.length === 0) {
                return false;
            }

            const first = payload[0];

            return first
                && Object.prototype.hasOwnProperty.call(first, 'id')
                && Object.prototype.hasOwnProperty.call(first, 'name')
                && Object.prototype.hasOwnProperty.call(first, 'family')
                && Object.prototype.hasOwnProperty.call(first, 'age')
                && Object.prototype.hasOwnProperty.call(first, 'gender');
        }
    });
}

/**
 * Verifies a generic successful HTTP response.
 *
 * @param {object} response - The k6 HTTP response object.
 * @param {string} label - A human-readable label to distinguish checks in output.
 * @param {number} expectedStatusCode - The expected HTTP status code.
 * @returns {boolean} True when all checks pass; otherwise false.
 */
export function checkStatusCode(response, label, expectedStatusCode) {
    return check(response, {
        [`${label} - status is ${expectedStatusCode}`]: (r) => r.status === expectedStatusCode
    });
}

/**
 * Safely parses a k6 response body as JSON.
 *
 * Returns null when parsing fails instead of throwing, which keeps checks stable.
 *
 * @param {object} response - The k6 HTTP response object.
 * @returns {object|null} Parsed JSON object, array, or null.
 */
function safeJsonParse(response) {
    try {
        return response.json();
    } catch {
        return null;
    }
}
