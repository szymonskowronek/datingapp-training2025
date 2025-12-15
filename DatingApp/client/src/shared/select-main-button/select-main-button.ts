import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-select-main-button',
  imports: [],
  templateUrl: './select-main-button.html',
  styleUrl: './select-main-button.css',
})
export class SelectMainButton {
  selected = input<boolean>();
  disabled = input<boolean>();
  select = output<Event>();

  onSelect(event: Event) {
    this.select.emit(event);
  }
}
