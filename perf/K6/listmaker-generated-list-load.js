import { sleep } from 'k6';
import http from 'k6/http';

import { getEnvironmentConfig } from './config/environments.js';
import { loginToListMaker } from './helpers/auth.js';
import { bearerJsonHeaders } from './helpers/headers.js';
import { checkPeopleListResponse } from './helpers/checks.js';
import { createCsvSummaryOutputs } from './helpers/csv-summary.js';

/**
 * k6 options for ListMaker.Api generated-list load testing.
 *
 * Purpose:
 * - Authenticate against ListMaker.Api.
 * - Call the protected generated-list endpoint directly.
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
 * setup() runs once before the load test starts.
 *
 * We authenticate once and share the token with VUs.
 *
 * Note:
 * This is suitable for baseline local load testing.
 * If we want to test login-per-user behavior later, we can move login into default().
 *
 * @returns {{accessToken: string}} Shared setup data.
 */
export function setup() {
    const accessToken = loginToListMaker(environment);

    return {
        accessToken
    };
}

/**
 * Main k6 virtual-user function.
 *
 * Each iteration:
 * - calls ListMaker.Api generated-list endpoint using bearer token
 * - validates the people-list response contract
 *
 * @param {{accessToken: string}} data - Setup data.
 */
export default function (data) {
    const generatedListUrl = `${environment.listMakerBaseUrl}/api/lists/generated`;

    const response = http.get(
        generatedListUrl,
        bearerJsonHeaders(data.accessToken)
    );

    checkPeopleListResponse(response, 'ListMaker generated-list load');

    sleep(1);
}

/**
 * Writes CSV summary files after the test run.
 *
 * @param {object} data - k6 summary data.
 * @returns {object} k6 summary output map.
 */
export function handleSummary(data) {
    return createCsvSummaryOutputs(data, 'listmaker-generated-list-load');
}
