/**
 * Provides reusable HTTP header builders for k6 scripts.
 */

/**
 * Builds headers for JSON requests without authentication.
 *
 * @returns {{headers: object}} A k6-compatible params object containing headers.
 */
export function jsonHeaders() {
    return {
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
    };
}

/**
 * Builds headers for JSON requests authenticated with a bearer token.
 *
 * @param {string} accessToken - The JWT bearer token.
 * @returns {{headers: object}} A k6-compatible params object containing headers.
 */
export function bearerJsonHeaders(accessToken) {
    return {
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Authorization': `Bearer ${accessToken}`
        }
    };
}