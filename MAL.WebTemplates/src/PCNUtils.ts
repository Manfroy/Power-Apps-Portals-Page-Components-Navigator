class PCNUtils {
  /**
   * Adds parent component id to the child's array of ancestors and then sets hasParent to true
   * @param child Child component
   * @param parent Parent Component
   */
  static addParentAndAncestorsToChildAncestors(
    child: PageComponent,
    parent: PageComponent | undefined
  ): void {
    try {
      if (parent) {
        [child.hasParent, child.ancestors] = [
          true,
          [...new Set([...child.ancestors, ...parent.ancestors, parent.id])],
        ];
      }
    } catch (err) {
      let customError = new Error(
        `${err.message} at function addParentAndAncestorsToChildAncestors}`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns true if child component is included on parent component
   * @param child Child component
   * @param parent Parent Component
   */
  static includedInParentSource(
    child: PageComponent,
    parent: PageComponent
  ): boolean {
    try {
      const childElementName = child.text.toLowerCase().replace(/ /g, "");
      const childElementId = child.id.toLowerCase();
      const [pageWebformId, pageEntityformId, pageEntitylistId] = [
        sessionStorage.getItem("MAL.PCN.page_adx_webform_id"),
        sessionStorage.getItem("MAL.PCN.page_adx_entityform_id"),
        sessionStorage.getItem("MAL.PCN.page_adx_entitylist_id"),
      ];

      return [
        `%include${childElementName}`,
        `%webformname:${childElementName}`,
        `%entityformname:${childElementName}`,
        `%webformid:${childElementId}`,
        `%entityformid:${childElementId}`,
        "%webformid:page.adx_webform.id",
        "%entityformid:page.adx_entityform.id",
        "%entitylistid:page.adx_entitylist.id",
        "%entitylistkey:page.adx_entitylist.id",
        "%includeentity_listkey:page.adx_entitylist.id",
      ].some(
        (v, i) =>
          parent.source.includes(v) &&
          (i <= 4 ||
            (i == 5 && pageWebformId == childElementId) ||
            (i == 6 && pageEntityformId == childElementId) ||
            (i >= 7 && pageEntitylistId == childElementId))
      );
    } catch (err) {
      let customError = new Error(
        `${err.message} at function includedInParentSource`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns true if both components have common ancestors
   * @param child Child component
   * @param parent Parent component
   */
  static commonAncestors(child: PageComponent, parent: PageComponent): boolean {
    try {
      const combinedAncestors = [...child.ancestors, ...parent.ancestors];
      return combinedAncestors.length != new Set(combinedAncestors).size;
    } catch (err) {
      let customError = new Error(`${err.message} at function commonAncestors`);
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns true if child is included in Parent source and the child isn't already an ancestor of parent. This avoids circular references.
   * @param child Child component
   * @param parent Parent component
   */
  static includedInParentSourceAndNotAlreadyAnAncestor(
    child: PageComponent,
    parent: PageComponent
  ): boolean {
    try {
      return (
        this.includedInParentSource(child, parent) &&
        !parent.ancestors.includes(child.id)
      );
    } catch (err) {
      let customError = new Error(
        `${err.message} at function includedInParentSourceAndNotAlreadyAnAncestor`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns true if child is already on array of nodes of parent
   * @param child Child Component
   * @param parent Parent Component
   */
  static isChild(child: PageComponent, parent: PageComponent): boolean {
    try {
      return parent.nodes.some((n) => n.id == child.id);
    } catch (err) {
      let customError = new Error(`${err.message} at function isChild`);
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns parent component after creating its tree of descendants components
   * @param pageComponents Array of all page components
   * @param parent Component for which tree of descendants will be created
   * @param parentIsOnExtendingTree Determines if component is on a subtree of a component being extended
   */
  static getNestedChildren(
    parent: PageComponent,
    pageComponents: Array<PageComponent>,
    parentIsOnExtendingTree = false
  ): PageComponent {
    try {
      pageComponents
        .filter(
          (pc) =>
            pc.id != parent.id &&
            !this.isChild(pc, parent) &&
            !(this.commonAncestors(pc, parent) && parentIsOnExtendingTree) &&
            this.includedInParentSourceAndNotAlreadyAnAncestor(pc, parent)
        )
        .forEach((pc) => {
          this.addParentAndAncestorsToChildAncestors(pc, parent);
          parent.nodes.push(
            this.getNestedChildren(pc, pageComponents, parentIsOnExtendingTree)
          );
        });
      return parent;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function getNestedChildren`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Assigns web form step order and creates subtree of components for web form step if any
   * @param webFormStepsArray Array of web form steps
   * @param webTemplates Array of all web templates on the page
   * @param formStepId Web Form Step to be processed
   * @param count Number used to set the order of the step
   */
  static getWebFormStepsFromWebForm(
    formStepId: string,
    webFormStepsArray: Array<WebFormStep>,
    webTemplates: Array<PageComponent>,
    count = 1
  ): void {
    try {
      if (count <= webFormStepsArray.length) {
        //avoids errors where step next step is a previous step
        webFormStepsArray
          .filter((wfs) => wfs.id == formStepId)
          .forEach((wfs) => {
            wfs.order = count++;
            this.getWebFormStepsFromWebForm(
              wfs.nextStep,
              webFormStepsArray,
              webTemplates,
              count
            );
            this.getNestedChildren(wfs, webTemplates);
          });
      }
    } catch (err) {
      let customError = new Error(
        `${err.message} at function getWebFormStepsFromWebForm`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Creates subtree of components for web templates that extend another
   * @param allWebTemplatesWithBlocks Array of all web templates extending another template
   * @param allPageComponents Array of all page components
   * @param extendingTemplate Extending template for which subtree of components will be created
   */
  static getTreeOfWebTemplatesBeingExtendedByMainWebTemplate(
    extendingTemplate: PageComponent,
    allWebTemplatesWithBlocks: Array<PageComponent>,
    allPageComponents: Array<PageComponent>
  ): PageComponent {
    try {
      extendingTemplate = this.getNestedChildren(
        extendingTemplate,
        allPageComponents,
        true
      );

      allWebTemplatesWithBlocks
        .filter((wt) =>
          extendingTemplate.source.includes(
            `%extends${wt.text.toLowerCase().replace(/ /g, "")}`
          )
        )
        .forEach((wt) => {
          this.addParentAndAncestorsToChildAncestors(wt, extendingTemplate);
          extendingTemplate.nodes.unshift(
            this.getTreeOfWebTemplatesBeingExtendedByMainWebTemplate(
              wt,
              allWebTemplatesWithBlocks,
              allPageComponents
            )
          );
        });

      return extendingTemplate;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function getTreeOfWebTemplatesBeingExtendedByMainWebTemplate`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Retrieves GUIDs of entity forms used by entity lists and subgrids to display as bootstrap modals
   */
  static retrieveAllModalEntityForms(): Array<string> {
    try {
      const modalEntityFormsIDs: Array<string> = [];

      document
        .querySelectorAll(".entity-grid.subgrid, .entity-grid.entitylist")
        .forEach((val) => {
          let attributeWithFormIds =
            val.getAttribute("data-view-layouts") ?? "";
          try {
            // Entity lists have this attribute encoded so trying the decoding here in case of entity list
            attributeWithFormIds = atob(attributeWithFormIds);
          } catch {
            // Element was a subgrid which do not have the attribute encoded
          }

          let ids;
          const regex = RegExp(
            '"EntityForm":{"Id":"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}',
            "g"
          );
          while ((ids = regex.exec(attributeWithFormIds)) !== null) {
            modalEntityFormsIDs.push(ids[0].substr(20, 36));
          }
        });

      return [...new Set(modalEntityFormsIDs)];
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveAllModalEntityForms`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Retrieves GUIDs of entity forms being rendered directly from a web template
   */
  static retrieveAllRegularEntityForms(): Array<string> {
    try {
      const eForms = document.querySelectorAll("[id^=EntityFormControl]");
      return [...new Set(Array.from(eForms).map((ef) => ef.id.substr(18, 32)))];
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveAllRegularEntityForms`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns array of the GUIDs of all web templates that show in HTML comments
   */
  static retrieveGuidsOfWebTemplatesInComments(): Array<string | undefined> {
    try {
      const webTemplateGUIDS: Array<string | undefined> = [];
      const iterator = document.createNodeIterator(
        document.body,
        NodeFilter.SHOW_COMMENT
      );
      let curNode;
      while ((curNode = iterator.nextNode())) {
        if (curNode.nodeValue?.includes("MAL.PCN.WebTemplateId=")) {
          webTemplateGUIDS.push(
            curNode.nodeValue.match(
              /[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}/g
            )?.[0]
          );
        }
      }
      return webTemplateGUIDS;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveGuidsOfWebTemplatesInComments`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns array of the GUIDs of all web templates inside script tags
   */
  static retrieveWebTemplateGUIDsInScripts(): Array<string | undefined> {
    try {
      let webTemplateGUIDS: Array<string | undefined> = [];
      Array.from(document.querySelectorAll("script"))
        .filter(
          (s) =>
            s.id != "malPcnJavascriptWrapperScript" &&
            s.innerHTML.includes("MAL.PCN.WebTemplateId=")
        )
        .forEach((s) => {
          webTemplateGUIDS = webTemplateGUIDS.concat(
            s.innerHTML
              .match(
                /MAL.PCN.WebTemplateId=[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}/g
              )
              ?.map((v: string) => v.replace("MAL.PCN.WebTemplateId=", ""))
          );
        });

      return webTemplateGUIDS;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveWebTemplateGUIDsInScripts`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns array of GUIDs of all web templates
   */
  static retrieveAllWebTemplates(): Array<string | undefined> {
    try {
      const webTemplates = [
        ...this.retrieveGuidsOfWebTemplatesInComments(),
        ...this.retrieveWebTemplateGUIDsInScripts(),
      ];
      return [...new Set(webTemplates)];
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveAllWebTemplates`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns array of GUIDs of all web forms
   */
  static retrieveWebForms(): Array<string> {
    try {
      const wForms = document.querySelectorAll("[id^=WebFormControl]");
      return [...new Set(Array.from(wForms).map((wf) => wf.id.substr(15, 32)))];
    } catch (err) {
      let customError = new Error(
        `${err.message} at function retrieveWebForms`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns the URL used to fetch component data
   */
  static createEndPointRequestURL(): string {
    try {
      const queryStringParameters = [
        `wp=${sessionStorage.getItem("MAL.PCN.page_adx_webpageid")}`,
        `pcnwt=${this.retrieveAllWebTemplates().join("_")}`,
        `pcnef=${this.retrieveAllRegularEntityForms().join("_")}`,
        `pcnmef=${this.retrieveAllModalEntityForms().join("_")}`,
        `pcnel=${sessionStorage.getItem("MAL.PCN.page_adx_entitylist_id")}`,
        `pcnwf=${this.retrieveWebForms().join("_")}`,
        `pcnwfstepid=${sessionStorage.getItem(
          "MAL.PCN.request_params_stepid"
        )}`,
      ];
      const queryString = queryStringParameters
        .filter((qsp) => !qsp.endsWith("="))
        .join("&");
      const endPointUrl = sessionStorage.getItem(
        "MAL.PCN.GetPageComponentsData"
      );

      return `${endPointUrl}?${queryString}`;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createEndPointRequestURL`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Creates Web Form Tree
   * @param webForms
   * @param allSourceComponents
   * @param webTemplates
   */
  static createWebFormTree(
    webForms: PageComponent[],
    allSourceComponents: PageComponent[],
    webTemplates: PageComponent[]
  ) {
    try {
      if (webForms.length) {
        allSourceComponents.push(webForms[0]);
        const webFormStartStep = webForms[0].nodes.find(
          (wfstep: WebFormStep) => wfstep.isStartStep
        );

        this.getWebFormStepsFromWebForm(
          webFormStartStep.id,
          webForms[0].nodes,
          webTemplates
        );

        webForms[0].nodes.sort(function (a: WebFormStep, b: WebFormStep) {
          return a.order - b.order;
        });

        webForms[0].nodes
          .filter((wfstep: WebFormStep) => wfstep.order == 100)
          .forEach((wfstep: WebFormStep) => {
            wfstep.text += " - NOT LINKED";
            wfstep.color = "red";
          });
      }
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createWebFormTree`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Creates tree for the header, main and footer web templates
   * @param headerWebTemplate Header web template component
   * @param mainWebTemplate Main web template component
   * @param footerWebTemplate Footer web template component
   * @param components Array of all components
   */
  static createHeaderAndFooterTrees(
    headerWebTemplate: PageComponent,
    footerWebTemplate: PageComponent,
    components: PageComponent[]
  ) {
    try {
      return [
        this.getNestedChildren(headerWebTemplate, components),
        this.getNestedChildren(footerWebTemplate, components),
      ];
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createHeaderAndFooterTrees`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   *
   * @param mainWebTemplate Main web template component
   * @param webTemplatesWithBlocks Array of all web extending/extended web templates
   * @param components Array of all components
   */
  private static createMainWebTemplateTree(
    mainWebTemplate: PageComponent,
    webTemplatesWithBlocks: PageComponent[],
    components: PageComponent[]
  ): PageComponent {
    try {
      const mainTree = this.getNestedChildren(mainWebTemplate, components);
      if (mainWebTemplate.source.includes("%extends")) {
        this.getTreeOfWebTemplatesBeingExtendedByMainWebTemplate(
          mainTree,
          webTemplatesWithBlocks,
          components
        );
      }
      return mainTree;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createMainWebTemplateTree`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Creates tree(s) of remaining components
   * @param allSourceComponents Array of all components
   * @param webTemplatesWithBlocks Array of all extending/extended web templates
   */
  static createTreeOfRemainingComponents(
    allSourceComponents: PageComponent[],
    webTemplatesWithBlocks: PageComponent[]
  ): Array<PageComponent> {
    try {
      const componentsWithParent: Array<string> = [
        ...allSourceComponents,
        ...webTemplatesWithBlocks,
      ]
        .filter((c) => c.hasParent)
        .map((c) => c.id);

      // Remaining orphans are those without parents and those whose id is not the same as a component with parent
      const remainingOrphanComponents: Array<PageComponent> = allSourceComponents.filter(
        (c) => !c.hasParent && !componentsWithParent.includes(c.id)
      );

      // Finds the orphans that are not referenced on any other component
      const rootOrphans = remainingOrphanComponents.filter(
        (c, i, arr) => !arr.find((p) => this.includedInParentSource(c, p))
      );

      rootOrphans.forEach((c) =>
        this.getNestedChildren(c, remainingOrphanComponents)
      );

      return rootOrphans;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createTreeOfRemainingComponents`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns all the data needed to create tree of page components
   */
  static async getData() {
    try {
      const response = await fetch(
        new URL(this.createEndPointRequestURL(), location.href).toString()
      );
      const results = await response.json();

      return results;
    } catch (err) {
      let customError = new Error(`${err.message} at function getData`);
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Creates the tree of components for the web page
   * @param mainWebPage Web page
   * @param mainWebTemplateTree Main Web Template tree
   * @param allSourceComponents Array of all components
   */
  static createWebPageTree(
    mainWebPage: PageComponent,
    mainWebTemplateTree: PageComponent,
    allSourceComponents: PageComponent[]
  ): PageComponent {
    try {
      mainWebPage.nodes.push(mainWebTemplateTree);
      this.getNestedChildren(
        mainWebPage,
        allSourceComponents.filter((c) => !c.hasParent)
      );
      return mainWebPage;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createWebPageTree`
      );
      customError.name = err.name;
      throw customError;
    }
  }

  /**
   * Returns the full tree of components on a page
   */
  static createComponentsTree(jsonData: any) {
    try {
      const {
        webTemplates,
        mainWebTemplate,
        footerWebTemplate,
        headerWebTemplate,
        modalEntityForms = [],
        regularEntityForms = [],
        entityLists = [],
        webForms = [],
        webTemplatesWithBlocks = [],
        mainWebPage,
      }: {
        webTemplates: Array<PageComponent>;
        mainWebTemplate: PageComponent;
        footerWebTemplate: PageComponent;
        headerWebTemplate: PageComponent;
        modalEntityForms: Array<PageComponent>;
        regularEntityForms: Array<PageComponent>;
        entityLists: Array<PageComponent>;
        webForms: Array<PageComponent>;
        webTemplatesWithBlocks: Array<PageComponent>;
        mainWebPage: PageComponent;
      } = jsonData;

      const allSourceComponents = [
        ...webTemplates,
        ...regularEntityForms,
        ...entityLists,
      ];

      this.createWebFormTree(webForms, allSourceComponents, webTemplates);

      const [headerTree, footerTree] = this.createHeaderAndFooterTrees(
        headerWebTemplate,
        footerWebTemplate,
        allSourceComponents
      );

      const mainWebTemplateTree = this.createMainWebTemplateTree(
        mainWebTemplate,
        webTemplatesWithBlocks,
        allSourceComponents
      );

      const webPageTree = this.createWebPageTree(
        mainWebPage,
        mainWebTemplateTree,
        allSourceComponents
      );

      const modalEntityFormsTree = {
        text: "Modal Entity Forms",
        backColor: "lightgray",
        state: { expanded: true },
        nodes: modalEntityForms,
      };

      const orphanOrDynamicallyInserted = {
        text: "Components with unresolved parents or dynamically referenced",
        backColor: "lightgray",
        state: { expanded: true },
        nodes: this.createTreeOfRemainingComponents(
          allSourceComponents,
          webTemplatesWithBlocks
        ),
      };

      const fullTree = [
        headerTree,
        webPageTree,
        orphanOrDynamicallyInserted,
        modalEntityFormsTree,
        footerTree,
      ];

      if (!orphanOrDynamicallyInserted.nodes.length) {
        fullTree.splice(fullTree.length - 3, 1);
      }
      if (!modalEntityFormsTree.nodes.length) {
        fullTree.splice(fullTree.length - 2, 1);
      }
      return fullTree;
    } catch (err) {
      let customError = new Error(
        `${err.message} at function createComponentsTree`
      );
      customError.name = err.name;
      throw customError;
    }
  }
}
