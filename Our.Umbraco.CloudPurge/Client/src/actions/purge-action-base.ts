import { UmbEntityActionBase } from "@umbraco-cms/backoffice/entity-action";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { umbConfirmModal } from "@umbraco-cms/backoffice/modal";

export class CloudPurgeActionBase extends UmbEntityActionBase<never> {
  protected async purge(descendants: boolean): Promise<void> {
    if (!this.args.unique) return;

    try {
      await umbConfirmModal(this, {
        headline: "Purge Cloudflare cache?",
        content: descendants ? "Purge this page and its descendants?" : "Purge this page?",
        color: "danger",
        confirmLabel: "Purge",
      });
    } catch {
      return;
    }

    const notifications = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    try {
      const auth = await this.getContext(UMB_AUTH_CONTEXT);
      const token = await auth?.getLatestToken();
      if (!token) throw new Error("Backoffice authentication is unavailable.");

      const url = `/umbraco/management/api/v1/cloudpurge/content/${encodeURIComponent(this.args.unique)}?descendants=${descendants}`;
      const response = await fetch(url, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!response.ok) throw new Error("Cloudflare purge failed.");

      const result = (await response.json()) as { purgedUrls: number };
      notifications?.peek("positive", {
        data: { headline: "CloudPurge", message: `${result.purgedUrls} URL(s) submitted to Cloudflare.` },
      });
    } catch {
      notifications?.peek("danger", {
        data: { headline: "CloudPurge", message: "Could not purge Cloudflare cache." },
      });
    }
  }
}
