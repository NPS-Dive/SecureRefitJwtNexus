import { sleep } from 'k6';
import http from 'k6/http';

import { getEnvironmentConfig } from './config/environments.js';
import { loginToListReader } from './helpers/auth.js';
import { bearerJsonHeaders } from './helpers/headers.js';
import { checkPeopleListResponse } from './helpers/checks.js';
import { createCsvSummaryOutputs } from './helpers/csv-summary.js';

/**
 * k6 options for ListReader.Api relay load testing.
 *
 * Purpose:
 * - Authenticate against ListReader.Api.
 * - Call ListReader.Api generated-list endpoint.
 * - ListReader.Api internally calls ListMaker.Api through Refit/IHttpClientFactory.
 *
 * This path is expected to be slower than direct ListMaker.Api calls because it includes:
 * - incoming ListReader request
 * - ListReader authentication/authorization
 * - downstream token handling
 * - Refit HTTP call to ListMaker.Api
 * - ListMaker response
 * - ListReader response mapping
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
        http_req_failed: ['rate<0.02'],
        http_req_duration: ['p(95)<1000'],
        checks: ['rate>0.98']
    }
};

const environment = getEnvironmentConfig();

/**
 * setup() runs once before the load test starts.
 *
 * We authenticate against ListReader.Api once and share the token with VUs.
 *
 * @returns {{accessToken: string}} Shared setup data.
 */
export function setup() {
    const accessToken = loginToListReader(environment);

    return {
        accessToken
    };
}

/**
 * Main k6 virtual-user function.
 *
 * Each iteration:
 * - calls ListReader.Api generated-list relay endpoint
 * - validates the relayed people-list response contract
 *
 * @param {{accessToken: string}} data - Setup data.
 */
export default function (data) {
    const relayUrl = `${environment.listReaderBaseUrl}/api/lists/generated`;

    const response = http.get(
        relayUrl,
        bearerJsonHeaders(data.accessToken)
    );

    checkPeopleListResponse(response, 'ListReader relay load');

    sleep(1);
}

/**
 * Writes CSV summary files after the test run.
 *
 * @param {object} data - k6 summary data.
 * @returns {object} k6 summary output map.
 */
export function handleSummary(data) {
    return createCsvSummaryOutputs(data, 'listreader-relay-load');
}
