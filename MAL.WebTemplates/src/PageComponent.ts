interface PageComponent {
  id: string;
  text: string;
  state: any;
  nodes: Array<any>;
  backColor: string;
  hasParent: boolean;
  ancestors: Array<string>;
  href: string;
  color: string;
  source: string;
}
