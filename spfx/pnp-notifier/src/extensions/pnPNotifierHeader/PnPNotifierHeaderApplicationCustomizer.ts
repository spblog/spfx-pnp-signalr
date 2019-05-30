import { override } from '@microsoft/decorators';
import { Log } from '@microsoft/sp-core-library';
import {
  BaseApplicationCustomizer, PlaceholderContent, PlaceholderName
} from '@microsoft/sp-application-base';
import * as React from 'react';
import * as ReactDom from 'react-dom';

import { SignalRHeader, IProps } from "./../components/SignalRHeader";

import * as strings from 'PnPNotifierHeaderApplicationCustomizerStrings';

const LOG_SOURCE: string = 'PnPNotifierHeaderApplicationCustomizer';

export interface IPnPNotifierHeaderApplicationCustomizerProperties {
  // This is an example; replace with your own property
  testMessage: string;
}

/** A Custom Action which can be run during execution of a Client Side Application */
export default class PnPNotifierHeaderApplicationCustomizer
  extends BaseApplicationCustomizer<IPnPNotifierHeaderApplicationCustomizerProperties> {

  private topPlaceholder: PlaceholderContent | undefined;

  @override
  public onInit(): Promise<void> {
    Log.info(LOG_SOURCE, `Initialized ${strings.Title}`);

    let message: string = this.properties.testMessage;
    if (!message) {
      message = '(No properties were provided.)';
    }

    if (!this.topPlaceholder) {
      this.topPlaceholder =
        this.context.placeholderProvider.tryCreateContent(
          PlaceholderName.Top,
          { onDispose: () => { } });

      // The extension should not assume that the expected placeholder is available.
      if (!this.topPlaceholder) {
        console.error('The expected placeholder (Top) was not found.');
        return;
      }

      const element: React.ReactElement<IProps> = React.createElement(
        SignalRHeader, {
          webUrl: this.context.pageContext.web.absoluteUrl
        });

      ReactDom.render(element, this.topPlaceholder.domElement);
    }

    return Promise.resolve();
  }
}
