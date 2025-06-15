import React, { Component } from "react";

export class FetchEsChartData extends Component {
    static displayName = FetchEsChartData.name;

    constructor(props) {
        super(props);
        this.state = { esChartData: [], loading: true };
    }

    componentDidMount() {
        this.populateEsChartData();
    }

    static renderEsChartDataTable(esChartData) {
        return (
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                    <tr>
                        <th>Open</th>
                        <th>Close</th>
                        <th>Low</th>
                        <th>High</th>
                        <th>Volume</th>
                    </tr>
                </thead>
                <tbody>
                    {esChartData.map(esChartData =>
                        <tr key={esChartData.ChartTime}>
                            <td>{esChartData.ChartTime}</td>
                            <td>{esChartData.Open}</td>
                            <td>{esChartData.Close}</td>
                            <td>{esChartData.Low}</td>
                            <td>{esChartData.High}</td>
                            <td>{esChartData.Volume}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : FetchEsChartData.renderEsChartDataTable(this.state.esChartData);

        return (
            <div>
                <h1 id="tableLabel">ES Chart</h1>
                <p>This component demonstrates fetching data from the server.</p>
                {contents}
            </div>
        );
    }

    async populateEsChartData() {
        const response = await fetch("escandlechart");
        console.log(`Controller returned: ${response}`);
        const data = await response.json();
        this.setState({ esChartData: data, loading: false });
    }
}
