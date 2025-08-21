import { Component, input, Input, output } from '@angular/core';

@Component({
  selector: 'app-favourite-button',
  imports: [],
  templateUrl: './favourite-button.html',
  styleUrl: './favourite-button.css'
})
export class FavouriteButton {
  disabled = input<boolean>();
  active = input<boolean>();
  clickEvent = output<Event>();

  onClick(event: Event) {
      this.clickEvent.emit(event);
  }
}
