/**
* Defines environment-specific configuration for k6 load testing.
*
* Notes:
* - This demo project currently targets a local development environment.
* - Additional environments can be added later, such as qa, staging, or docker.
* - Credentials intentionally match the static users configured in the APIs.
*/

export const environments = {
    local: {
        /**
         * Base URL of ListMaker.Api.
         */
        listMakerBaseUrl: 'https://localhost:7001',

        /**
         * Base URL of ListReader.Api.
         */
        listReaderBaseUrl: 'https://localhost:7002',

        /**
         * Static login credentials for ListMaker.Api.
         */
        listMakerUsername: '@maker-service-user',
        listMakerPassword: '@maker-service-password',

        /**
         * Static login credentials for ListReader.Api.
         */
        listReaderUsername: 'reader@admin',
        listReaderPassword: 'Reader@Pass_@123!'
    }
};

/**
 * Gets the active environment name from the K6_ENV environment variable.
 * Falls back to "local" when not provided.
 *
 * Example:
 *   k6 run -e K6_ENV=local script.js
 *
 * @returns {string} The active environment name.
 */
export function getEnvironmentName() {
    return __ENV.K6_ENV || 'local';
}

/**
 * Resolves the active environment configuration object.
 *
 * @returns {object} The active environment configuration.
 * @throws {Error} Thrown when the requested environment does not exist.
 */
export function getEnvironmentConfig() {
    const environmentName = getEnvironmentName();
    const environment = environments[environmentName];

    if (!environment) {
        throw new Error(
            `Unknown k6 environment "${environmentName}". ` +
            `Available environments: ${Object.keys(environments).join(', ')}.`
        );
    }

    return environment;
}