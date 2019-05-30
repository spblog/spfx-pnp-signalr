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

    connection.on("initial-state", (message, progress, total) => {
      if (total === -1) {
        this.setState({
          width: 5,
          message: 'Starting...'
        });
      } else {
        this.setState({
          width: progress / total * 100,
          message: `Processing: ${message}`
        });
      }
      console.log(message);
      console.log(progress);
      console.log(total);
    });

    connection.on("notify", data => {
      console.log(data);

      this.setState({
        width: data.Progress / data.Total * 100,
        message: `Processing: ${data.Message}`
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
