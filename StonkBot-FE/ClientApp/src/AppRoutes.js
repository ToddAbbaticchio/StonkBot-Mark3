import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { FetchEsChartData } from "./components/FetchEsChartData";
import { Home } from "./components/Home";

const AppRoutes = [
    {
        index: true,
        element: <Home />
    },
    {
        path: "/counter",
        element: <Counter />
    },
    {
        path: "/fetch-data",
        element: <FetchData />
    },
    {
        path: "/esChart",
        element: <FetchEsChartData />
    }
];

export default AppRoutes;
