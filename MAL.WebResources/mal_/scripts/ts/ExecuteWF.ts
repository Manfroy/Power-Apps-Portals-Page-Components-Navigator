class MAL_PCN_ExecuteWF {
  /**
   * Executes the workflow that creates data components required.
   * @param formContext Form context
   * @param recordId Id of workflow
   */
  static async enablePCN(formContext: Xrm.FormContext, recordId: string) {
    const notificationId = 'MAL_PCN_ExecuteWF.enablePCN';

    try {
      formContext.ui.setFormNotification(
        'Page Components Navigator is being enabled. This setup can take a few minutes. A confirmation message will show once enabled.',
        'INFO',
        notificationId
      );

      const request = new ExecuteWorkflowRequest(
        recordId,
        '9b4c93aa-0227-436d-a73f-e0c13c058d5b'
      );

      await Xrm.WebApi.online.execute(request);
      await Xrm.Navigation.openAlertDialog(
        { text: 'Page Components Navigator is now enabled on this site.' },
        { width: 330, height: 126 }
      );
    } catch (e) {
      Xrm.Navigation.openErrorDialog(e);
    } finally {
      const appURL = Xrm.Utility.getGlobalContext().getCurrentAppUrl();

      if (appURL.includes('appid')) {
        Xrm.WebApi.online.updateRecord(
          'adx_sitesetting',
          'd4ed9c23-6020-490a-a82c-65255b4987bc',
          { adx_value: appURL }
        );
      }

      formContext.ui.clearFormNotification(notificationId);
    }
  }

  /**
   * Executes the workflow that deletes data components and removes web templates id from web templates source.
   * @param formContext Form context
   * @param recordId Id of workflow
   */
  static async uninstallPCN(formContext: Xrm.FormContext, recordId: string) {
    const notificationId = 'MAL_PCN_EnablePCN.uninstallPCN';

    try {
      formContext.ui.setFormNotification(
        'Page Components Navigator is being uninstalled. This can take a few minutes. A confirmation message will show once uninstalled.',
        'INFO',
        notificationId
      );

      const request = new ExecuteWorkflowRequest(
        recordId,
        'a5404df8-be8b-46e8-8386-aeafed72e362'
      );

      await Xrm.WebApi.online.execute(request);
      await Xrm.Navigation.openAlertDialog(
        { text: 'Page Components Navigator has been uninstalled.' },
        { width: 330, height: 126 }
      );
    } catch (e) {
      Xrm.Navigation.openErrorDialog(e);
    } finally {
      formContext.ui.clearFormNotification(notificationId);
    }
  }
}

const enum operationType {
  Action = 0,
  Function = 1,
  CRUD = 2,
}

const enum structuralProperty {
  Unknown = 0,
  PrimitiveType = 1,
  ComplexType = 2,
  EnumerationType = 3,
  Collection = 4,
  EntityType = 5,
}

/**
 * Executes an on-demand workflow.
 * */
class ExecuteWorkflowRequest {
  EntityId: {
    guid: string;
  };

  entity: {
    id: string;
    entityType: string;
  };

  /**
   * @param entityId The ID of the record on which the workflow executes.
   * @param workflowId The workflow ID to execute.
   */
  constructor(entityId: string, workflowId: string) {
    this.entity = {
      id: workflowId,
      entityType: 'workflow',
    };
    this.EntityId = { guid: entityId };
  }

  getMetadata() {
    return {
      boundParameter: 'entity',
      operationName: 'ExecuteWorkflow',
      operationType: operationType.Action,
      parameterTypes: {
        entity: {
          typeName: 'Microsoft.Dynamics.CRM.ExecuteWorkflow',
          structuralProperty: structuralProperty.EntityType,
        },
        EntityId: {
          typeName: 'Edm.Guid',
          structuralProperty: structuralProperty.PrimitiveType,
        },
      },
    };
  }
}
