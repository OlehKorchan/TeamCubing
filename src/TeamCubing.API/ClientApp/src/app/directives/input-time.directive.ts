import { Directive, ElementRef, HostListener } from '@angular/core';
import { InputTimePipe } from '../pipes/input-time.pipe';

@Directive({
  selector: '[appInputTime]',
})
export class InputTimeDirective {
  private inputPipe = new InputTimePipe();

  public constructor(private el: ElementRef) {}

  @HostListener('focus')
  public onFocus() {
    const currentValue = this.el.nativeElement.value;
    this.el.nativeElement.value = this.inputPipe.parse(currentValue);
  }

  @HostListener('blur')
  public onInput() {
    const currentValue = this.el.nativeElement.value;
    const formattedValue = this.inputPipe.transform(currentValue);
    this.el.nativeElement.value = formattedValue;
  }
}
