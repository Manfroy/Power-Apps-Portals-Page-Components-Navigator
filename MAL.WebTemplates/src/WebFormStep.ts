interface WebFormStep extends PageComponent {
  order: number;
  nextStep: string;
  isStartStep: boolean;
}
