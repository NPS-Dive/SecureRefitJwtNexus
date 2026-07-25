import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';

/**
 * Safely reads a k6 metric value.
 *
 * @param {object} data - k6 summary data.
 * @param {string} metricName - Metric name.
 * @param {string} valueName - Metric value name.
 * @returns {string|number} Metric value or empty string.
 */
function metric(data, metricName, valueName) {
    return data.metrics[metricName]?.values?.[valueName] ?? '';
}

/**
 * Escapes a CSV value.
 *
 * @param {string|number} value - Value to escape.
 * @returns {string} CSV-safe value.
 */
function csv(value) {
    const text = String(value ?? '');
    return `"${text.replaceAll('"', '""')}"`;
}

/**
 * Builds a one-run CSV summary text for a single k6 execution.
 *
 * This creates a temporary current-run file:
 *
 *   results/{scriptName}.current.csv
 *
 * Then append-results.ps1 appends the data row into:
 *
 *   results/{scriptName}.csv
 *
 * @param {object} data - k6 summary data.
 * @param {string} scriptName - Logical load-test script name.
 * @returns {string} CSV text containing header + one data row.
 */
function buildSingleRunCsv(data, scriptName) {
    const timestamp = new Date().toISOString();

    const headerColumns = [
        'timestamp',
        'script',

        'checks_rate',

        'http_reqs_count',
        'http_req_failed_rate',

        'http_req_duration_avg',
        'http_req_duration_min',
        'http_req_duration_med',
        'http_req_duration_max',
        'http_req_duration_p90',
        'http_req_duration_p95',
        'http_req_duration_p99',

        'http_req_waiting_avg',
        'http_req_waiting_min',
        'http_req_waiting_med',
        'http_req_waiting_max',
        'http_req_waiting_p90',
        'http_req_waiting_p95',
        'http_req_waiting_p99',

        'http_req_blocked_avg',
        'http_req_connecting_avg',
        'http_req_tls_handshaking_avg',
        'http_req_sending_avg',
        'http_req_receiving_avg',

        'iterations_count',

        'iteration_duration_avg',
        'iteration_duration_min',
        'iteration_duration_med',
        'iteration_duration_max',
        'iteration_duration_p90',
        'iteration_duration_p95',
        'iteration_duration_p99',

        'vus_value',
        'vus_max_value',

        'data_received_count',
        'data_sent_count',

        'http_reqs_rate',
        'iterations_rate',
        'checks_passes',
        'checks_fails',
        'data_received_rate',
        'data_sent_rate',
        'test_run_duration_ms',
    ];

    const rowValues = [
        timestamp,
        scriptName,

        metric(data, 'checks', 'rate'),

        metric(data, 'http_reqs', 'count'),
        metric(data, 'http_req_failed', 'rate'),

        metric(data, 'http_req_duration', 'avg'),
        metric(data, 'http_req_duration', 'min'),
        metric(data, 'http_req_duration', 'med'),
        metric(data, 'http_req_duration', 'max'),
        metric(data, 'http_req_duration', 'p(90)'),
        metric(data, 'http_req_duration', 'p(95)'),
        metric(data, 'http_req_duration', 'p(99)'),

        metric(data, 'http_req_waiting', 'avg'),
        metric(data, 'http_req_waiting', 'min'),
        metric(data, 'http_req_waiting', 'med'),
        metric(data, 'http_req_waiting', 'max'),
        metric(data, 'http_req_waiting', 'p(90)'),
        metric(data, 'http_req_waiting', 'p(95)'),
        metric(data, 'http_req_waiting', 'p(99)'),

        metric(data, 'http_req_blocked', 'avg'),
        metric(data, 'http_req_connecting', 'avg'),
        metric(data, 'http_req_tls_handshaking', 'avg'),
        metric(data, 'http_req_sending', 'avg'),
        metric(data, 'http_req_receiving', 'avg'),

        metric(data, 'iterations', 'count'),

        metric(data, 'iteration_duration', 'avg'),
        metric(data, 'iteration_duration', 'min'),
        metric(data, 'iteration_duration', 'med'),
        metric(data, 'iteration_duration', 'max'),
        metric(data, 'iteration_duration', 'p(90)'),
        metric(data, 'iteration_duration', 'p(95)'),
        metric(data, 'iteration_duration', 'p(99)'),

        metric(data, 'vus', 'value'),
        metric(data, 'vus_max', 'value'),

        metric(data, 'data_received', 'count'),
        metric(data, 'data_sent', 'count'),

        metric(data, 'http_reqs', 'rate'),
        metric(data, 'iterations', 'rate'),
        metric(data, 'checks', 'passes'),
        metric(data, 'checks', 'fails'),
        metric(data, 'data_received', 'rate'),
        metric(data, 'data_sent', 'rate'),
        data.state?.testRunDurationMs ?? '',
    ];

    const header = headerColumns.map(csv).join(',');
    const row = rowValues.map(csv).join(',');

    return `${header}\n${row}\n`;
}

/**
 * Creates the console and temporary CSV summary outputs.
 *
 * @param {object} data - Complete k6 summary data.
 * @param {string} scriptName - Logical load-test script name.
 * @returns {object} k6 summary output destinations.
 */
export function createCsvSummaryOutputs(data, scriptName) {
    return {
        stdout: textSummary(data, {
            indent: ' ',
            enableColors: true
        }),

        [`./results/${scriptName}.current.csv`]:
            buildSingleRunCsv(data, scriptName)
    };
}

