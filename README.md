# Power Apps Portals Page Components Navigator

A tool that allows to see a hierarchical tree of components being used on a portal page and navigate directly to their records on a model driven app. The hierarchical tree is created using the [Bootstrap Tree View solution](https://github.com/jonmiles/bootstrap-treeview)

## Getting Started

### Setup

[Download](https://github.com/Manfroy/Power-Apps-Portals-Page-Components-Navigator/releases/download/v1.0.0.0/PageComponentsNavigator_1_0_0_0_managed.zip) the Page Components Navigator solution and install it.

Once the solution is installed, navigate to the portals website record for which you want to set up the Page Components Navigator and click on the "Enable PCN" button from the ribbon. Doing this will result on some [data modifications](https://github.com/Manfroy/Power-Apps-Portals-Page-Components-Navigator#data-modifications) and some [data additions](https://github.com/Manfroy/Power-Apps-Portals-Page-Components-Navigator#data-additions) so please read what those modifications are before enabling the functionality on the website.

Once enabled, clear the portal cache and reload/open a page. A PCN button will appear on the portal admin control that shows on the top right of the screen if the portal user you are currently logged in with has the OOB Administrators Web Role.

Note: The Model Driven App used to navigate to the website record to enable the functionality will be the one on which records will open once clicked upon from the portal.

### Usage

Either click the PCN button or press the Ctrl + Space key combination which will bring up a Bootstrap modal window with a hierarchical tree of the components being used on the page.

The following components will show on the tree of components if present on the page:

- Web Page
- Web Templates
- Entity Forms
- Entity Form Metadata
- Web Forms
- Web Form Steps
- Web Form Metadata
- Entity Lists

Clicking on a given commponent on the tree will open its record on the model driven app that was used to enable the functionality on the website.

## Data Modifications

- A new field named "Enabled for PCN" is added to the Web Template entity to determine if a web template should be enabled for the Page Components Navigator functionality. This field will be automatically set to Yes in the following instances:

  - Web Templates with at least one related Page Template record that uses the Website header and footer
  - Web Templates with no related Page Templates

- An HTML comment containing the web template id will be added to the end of web templates sources. This is done so that the web template ID shows on the HTML on the page so that it can retrieved and used to generate the tree structure. This happens to all web templates except for the following cases:

  - Web templates with a Page Template that does not use the Website header and footer. These web templates are most likely used as API endpoints to retrieve data. Adding an HTML comment to their source would break that functionality.

  - Web templates that extend another web template. These web templates can't have anything outside a liquid block tag. They are being rendered on the tree structure by going up the extending chain that starts with the page Main web template if it extends another web template.

- A new field named "Filtered Source" that will contain a stripped-down version of the source codes containing only the relevant lines of code required to create the tree of components is added to the following entities and populated accordingly:

  - Web Pages (combined stripped-down source from page copy and javascript section)
  - Entity Forms (combined stripped-down source from javascript section and instructions field)
  - Web Templates (stripped-down source of the web template source code)
  - Web Form Steps (stripped-down source of the javascript source)

- The following lines of liquid code are added at the end of the Website's footer web template.

```
{% assign isPCNEnabled = settings["MAL.PCN.EnablePCN"] | default: "false"%}
{% if isPCNEnabled == "true" %}
  {% include 'MAL.PCN.PCNJavascriptWrapper' %}
{% endif %}
```

Note: Switching the "Enabled for PCN" field to "Yes"/"No" will add/remove the HTML comment from the end of the web template source, therefore showing/not showing the web template on the tree of components.

Note: The web templates where the field "Enabled for PCN" are not set to "Yes" are intentionally left blank for the user to decide if it should or not be added to the tree of components based on the explanation provided above.

## Data Additions

- Read Entity permissions with global scope enabled only for contacts with the OOB portal administrator web role are created for the following entities:

  - Contact
  - Entity Form
  - Entity Form Metadata
  - Entity List
  - Site Marker
  - Web Form
  - Web Form Step
  - Web Form Metadata
  - Web Page
  - Web Role
  - Web Template

- Two Site Settings
- One Site Marker
- Three Web Templates
- One Page Template
- One root web page with its content web page
  - The web page is used in conjunction with the page template and one of the three web templates to create and API endpoint that provides the data needed to create the tree of components. The Site Marker that is created points to this web page. This allows to easily change the parent page to a different page without breaking any functionality. By default the parent page is set to the home page of the website.

Note: The name of all created records starts with "MAL.PCN."

## Known Limitations

Entity Lists that are referenced by name or directly by GUID on web templates won't show up on the tree of components. The code snippet below shows examples of this:

```
{% entitylist name: My Entity List %}
{% entitylist my_list = name:My Entity List %}
{% entitylist id:936DA01F-9ABD-4d9d-80C7-02AF85C822A8 %}
```

For them to show on the tree of components, they have to be refrenced in one of the following ways:

```
{% entitylist id:page.adx_entitylist.id %}
{% entitylist key:page.adx_entitylist.id %}
{% include entity_list key: page.adx_entitylist.id %}
```
