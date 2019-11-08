import * as React from 'react';
import * as signalR from "@aspnet/signalr";
import './SignalRHeader.scss';

export interface IProps {
  webUrl: string;
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
      signalRHubUrl = 'https://localhost:44341/pnpprovisioninghub';
    } else {
      signalRHubUrl = 'https://spfx-signalr-demo.azurewebsites.net/pnpprovisioninghub';
    }

    let connection = new signalR.HubConnectionBuilder()
      .withUrl(signalRHubUrl)
      .build();

      /*
    connection.on("initial-state", data => {
      console.log(data);

      if (data.total === -1) {
        this.setState({
          width: 5,
          message: 'Starting...'
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
*/
    connection.on("notify", data => {
      console.log(data);
      let width = data.progress / data.total * 100;
      let percent = Math.floor(width);
      this.setState({
        width,
        message: `${percent}% Provisioning: ${data.message}`
      });
    });

    connection.on("completed", () => {
      console.log("completed");
      this.setState({
        width: 100,
        message: `Your site is fully provisioned. Refresh the page to see the changes.`
      });
    });

    await connection.start();
    await connection.invoke("InitialState", this.props.webUrl);
  }

  public render(): React.ReactElement {
    let divStyle = {
      width: `${this.state.width}%`
    };

    return (
      <div className="demo-preview">
        <div className="progress progress-striped active">
          <div role="progressbar progress-striped" style={divStyle} className="progress-bar"><span>{this.state.message}</span></div>
        </div>
      </div>
    );
  }
}
