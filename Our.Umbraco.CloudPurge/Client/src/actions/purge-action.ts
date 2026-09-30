import { CloudPurgeActionBase } from "./purge-action-base.js";

export class CloudPurgeAction extends CloudPurgeActionBase {
  async execute(): Promise<void> {
    await this.purge(false);
  }
}

export { CloudPurgeAction as api };
