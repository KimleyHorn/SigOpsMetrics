// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  name: "dev",
  production: false,
  API_PATH: "https://localhost:44347/",
  mapCenterLat: 33.757776,
  mapCenterLon: -84.391578,
  hasPageOperations: true,
  hasPageMaintenance: true,
  hasPageWatchdog: true,
  hasPageTeamsTasks: true,
  hasPageReports: true,
  hasPageHealthMetrics: true,
  hasPageSummaryTrend: true,
  hasBtnContactUs: true,
  hasBtnGdotApplications: true,
  ttiGoal: 1.2,
  ptiGoal: 1.3,
  duGoal: 0.95,
  ppuGoal: 0.95,
  cctvGoal: 0.95,
  cuGoal: 0.95,
};

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
