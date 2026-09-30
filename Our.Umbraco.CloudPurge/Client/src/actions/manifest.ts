export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "entityAction",
    kind: "default",
    alias: "CloudPurge.PurgeDocument",
    name: "Purge Cloudflare cache for document",
    api: () => import("./purge-action.js"),
    forEntityTypes: ["document"],
    meta: {
      icon: "icon-cloud",
      label: "Purge Cloudflare cache",
    },
    conditions: [
      { alias: "Umb.Condition.UserPermission.Document", allOf: ["Umb.Document.Publish"] },
    ],
  },
  {
    type: "entityAction",
    kind: "default",
    alias: "CloudPurge.PurgeDocumentAndDescendants",
    name: "Purge Cloudflare cache for document and descendants",
    api: () => import("./purge-descendants-action.js"),
    forEntityTypes: ["document"],
    meta: {
      icon: "icon-cloud",
      label: "Purge page and descendants",
    },
    conditions: [
      { alias: "Umb.Condition.UserPermission.Document", allOf: ["Umb.Document.Publish"] },
    ],
  },
];
