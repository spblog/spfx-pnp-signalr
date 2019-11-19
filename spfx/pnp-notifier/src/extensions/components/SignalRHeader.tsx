import * as React from 'react';
import * as signalR from "@aspnet/signalr";
import './SignalRHeader.scss';
import { ApplicationCustomizerContext } from '@microsoft/sp-application-base';
import { clientId, localHubUrl, cloudHubUrl } from '../consts';
import { HttpTransportType } from '@aspnet/signalr';

export interface IProps {
  webUrl: string;
  context: ApplicationCustomizerContext;
}

interface IState {
  width: number;
  message: string;
}

export class SignalRHeader extends React.Component<IProps, IState> {

  constructor(props) {
    super(props);

    this.state = {
      width: 1,
      message: ''
    };
  }

  public async componentDidMount(): Promise<void> {
    let signalRHubUrl;
    if (process.env.NODE_ENV === 'dev') {
      signalRHubUrl = localHubUrl;
    } else {
      signalRHubUrl = cloudHubUrl;
    }

    let connection = new signalR.HubConnectionBuilder()
      .withUrl(signalRHubUrl, {
        accessTokenFactory: this.getAccessToken.bind(this), // authentication
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets // explicitly use web sockets in order to avoid CORS issues (web sockets are not subject of CORS)
      })
      .build();

    // when "initial-state" event is triggered from server
    connection.on("initial-state", data => {
      console.log(data);

      if (data.total === -1) {
        this.setState({
          width: 100,
          message: 'Preparing the engine... The provisioning will start in a few seconds...'
        });
      } else {
        let width = data.progress / data.total * 100;
        let percent = Math.floor(width);
        this.setState({
          width,
          message: `${percent}% Provisioning: ${data.message}`
        });
      }
    });

    // when "notify" event is triggered from server
    connection.on("notify", data => {
      console.log(data);
      let width = data.progress / data.total * 100;
      let percent = Math.floor(width);

      if (width >= 100) {
        width = 99;
        percent = 99;
      }

      this.setState({
        width,
        message: `${percent}% Provisioning: ${data.message}`
      });
    });

    // when "completed" event is triggered from server
    connection.on("completed", () => {
      console.log("completed");
      this.setState({
        width: 100,
        message: `Your site is fully provisioned. Refresh the page to see the changes.`
      });
    });

    // connect
    await connection.start();

    // and ask for the initial state of the provisioning
    await connection.invoke("InitialState", this.props.webUrl);
  }

  // gets Azure AD JWT tokens for authentication against SignalR server
  public async getAccessToken(): Promise<string> {
    let tokenProvider = await this.props.context.aadTokenProviderFactory.getTokenProvider();
    let token = await tokenProvider.getToken(clientId);

    console.log(token);

    return token;
  }

  public render(): React.ReactElement {
    let divStyle = {
      width: `${this.state.width}%`
    };

    return (
      <div className="demo-preview">
        <div className="progress progress-striped active">
          <div role="progressbar progress-striped" style={divStyle} className={this.state.width === 100 ? "regular" : "progress-bar"}><span>{this.state.message}</span></div>
        </div>
      </div>
    );
  }
}
