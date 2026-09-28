import { CloudPurgeActionBase } from './purge-action-base.js';

export class CloudPurgeDescendantsAction extends CloudPurgeActionBase {
  async execute() {
    await this.purge(true);
  }
}

export { CloudPurgeDescendantsAction as api };
