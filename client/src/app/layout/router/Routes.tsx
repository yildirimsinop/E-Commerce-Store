import { createBrowserRouter } from "react-router-dom";
import HomePage from "../../../features/home/Homepage";
import AboutPage from "../../../features/about/AboutPage";
import { ContactPage } from "@mui/icons-material";
import ProductDetails from "../../../features/catalog/ProductDetails";
import App from "../App";
import Catalog from "../../../features/catalog/Catalog";

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
            { path: '', element: <HomePage /> },
            { path: 'catalog', element: <Catalog /> },
            { path: 'catalog/:id', element: <ProductDetails /> },
            { path: 'about', element: <AboutPage /> },
            { path: 'contact', element: <ContactPage /> },
        ]
    }

])