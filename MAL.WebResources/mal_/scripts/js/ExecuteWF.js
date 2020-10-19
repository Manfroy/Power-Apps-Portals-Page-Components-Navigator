var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var MAL_PCN_ExecuteWF = /** @class */ (function () {
    function MAL_PCN_ExecuteWF() {
    }
    /**
     * Executes the workflow that creates data components required.
     * @param formContext Form context
     * @param recordId Id of workflow
     */
    MAL_PCN_ExecuteWF.enablePCN = function (formContext, recordId) {
        return __awaiter(this, void 0, void 0, function () {
            var notificationId, request, e_1, appURL;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        notificationId = 'MAL_PCN_ExecuteWF.enablePCN';
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 4, 5, 6]);
                        formContext.ui.setFormNotification('Page Components Navigator is being enabled. This setup can take a few minutes. A confirmation message will show once enabled.', 'INFO', notificationId);
                        request = new ExecuteWorkflowRequest(recordId, '9b4c93aa-0227-436d-a73f-e0c13c058d5b');
                        return [4 /*yield*/, Xrm.WebApi.online.execute(request)];
                    case 2:
                        _a.sent();
                        return [4 /*yield*/, Xrm.Navigation.openAlertDialog({ text: 'Page Components Navigator is now enabled on this site.' }, { width: 330, height: 126 })];
                    case 3:
                        _a.sent();
                        return [3 /*break*/, 6];
                    case 4:
                        e_1 = _a.sent();
                        Xrm.Navigation.openErrorDialog(e_1);
                        return [3 /*break*/, 6];
                    case 5:
                        appURL = Xrm.Utility.getGlobalContext().getCurrentAppUrl();
                        if (appURL.includes('appid')) {
                            Xrm.WebApi.online.updateRecord('adx_sitesetting', 'd4ed9c23-6020-490a-a82c-65255b4987bc', { adx_value: appURL });
                        }
                        formContext.ui.clearFormNotification(notificationId);
                        return [7 /*endfinally*/];
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    /**
     * Executes the workflow that deletes data components and removes web templates id from web templates source.
     * @param formContext Form context
     * @param recordId Id of workflow
     */
    MAL_PCN_ExecuteWF.uninstallPCN = function (formContext, recordId) {
        return __awaiter(this, void 0, void 0, function () {
            var notificationId, request, e_2;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        notificationId = 'MAL_PCN_EnablePCN.uninstallPCN';
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 4, 5, 6]);
                        formContext.ui.setFormNotification('Page Components Navigator is being uninstalled. This can take a few minutes. A confirmation message will show once uninstalled.', 'INFO', notificationId);
                        request = new ExecuteWorkflowRequest(recordId, 'a5404df8-be8b-46e8-8386-aeafed72e362');
                        return [4 /*yield*/, Xrm.WebApi.online.execute(request)];
                    case 2:
                        _a.sent();
                        return [4 /*yield*/, Xrm.Navigation.openAlertDialog({ text: 'Page Components Navigator has been uninstalled.' }, { width: 330, height: 126 })];
                    case 3:
                        _a.sent();
                        return [3 /*break*/, 6];
                    case 4:
                        e_2 = _a.sent();
                        Xrm.Navigation.openErrorDialog(e_2);
                        return [3 /*break*/, 6];
                    case 5:
                        formContext.ui.clearFormNotification(notificationId);
                        return [7 /*endfinally*/];
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    return MAL_PCN_ExecuteWF;
}());
/**
 * Executes an on-demand workflow.
 * */
var ExecuteWorkflowRequest = /** @class */ (function () {
    /**
     * @param entityId The ID of the record on which the workflow executes.
     * @param workflowId The workflow ID to execute.
     */
    function ExecuteWorkflowRequest(entityId, workflowId) {
        this.entity = {
            id: workflowId,
            entityType: 'workflow',
        };
        this.EntityId = { guid: entityId };
    }
    ExecuteWorkflowRequest.prototype.getMetadata = function () {
        return {
            boundParameter: 'entity',
            operationName: 'ExecuteWorkflow',
            operationType: 0 /* Action */,
            parameterTypes: {
                entity: {
                    typeName: 'Microsoft.Dynamics.CRM.ExecuteWorkflow',
                    structuralProperty: 5 /* EntityType */,
                },
                EntityId: {
                    typeName: 'Edm.Guid',
                    structuralProperty: 1 /* PrimitiveType */,
                },
            },
        };
    };
    return ExecuteWorkflowRequest;
}());
//# sourceMappingURL=ExecuteWF.js.map