import { sleep } from 'k6';
import http from 'k6/http';

import { getEnvironmentConfig } from './config/environments.js';
import { jsonHeaders } from './helpers/headers.js';
import { checkLoginResponse } from './helpers/checks.js';
import { createCsvSummaryOutputs } from './helpers/csv-summary.js';

/**
 * k6 options for ListMaker.Api login load testing.
 *
 * Purpose:
 * - Measure ListMaker.Api authentication endpoint under baseline local load.
 */
export const options = {
   stages: [
    { duration: '15s', target: 100 },
    { duration: '30s', target: 100 },

    { duration: '15s', target: 250 },
    { duration: '30s', target: 250 },

    { duration: '15s', target: 500 },
    { duration: '30s', target: 500 },

    { duration: '15s', target: 750 },
    { duration: '30s', target: 750 },

    { duration: '15s', target: 1000 },
    { duration: '30s', target: 1000 },

    { duration: '30s', target: 0 }
]
,
    summaryTrendStats: [
        'avg',
        'min',
        'med',
        'max',
        'p(90)',
        'p(95)',
        'p(99)'
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
        checks: ['rate>0.99']
    }
};

const environment = getEnvironmentConfig();

/**
 * Main k6 virtual-user function.
 *
 * Each iteration:
 * - sends a login request to ListMaker.Api
 * - validates HTTP 200
 * - validates accessToken exists
 */
export default function () {
    const loginUrl = `${environment.listMakerBaseUrl}/api/auth/login`;

    const requestBody = JSON.stringify({
        username: environment.listMakerUsername,
        password: environment.listMakerPassword
    });

    const response = http.post(loginUrl, requestBody, jsonHeaders());

    checkLoginResponse(response, 'ListMaker login load');

    sleep(1);
}

/**
 * Writes CSV summary files after the test run.
 *
 * @param {object} data - k6 summary data.
 * @returns {object} k6 summary output map.
 */
export function handleSummary(data) {
    return createCsvSummaryOutputs(data, 'listmaker-login-load');
}
