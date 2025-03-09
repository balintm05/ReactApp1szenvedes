/* eslint-disable no-unused-vars */
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Index from "./pages/Index.jsx";
import Layout from "./pages/layout/Layout.jsx";
import Login from "./pages/account/Login.jsx";
import PageNotFound from "./pages/errors/PageNotFound.jsx";
import * as React from 'react';
//import './index.css' //Te itten nekem ne rond�ts a k�d j� l�gyszi k�szike

export default function App() {
    return (
        <div>
            <h1>App </h1>
            <BrowserRouter>
                <Routes>
                    <Route element={<Layout/> }>
                        <Route index element={<Index />} />
                        <Route path="/account/login" element={<Login />} />
                        <Route path="*" element={<PageNotFound />} />
                    </Route>
                </Routes>
            </BrowserRouter>
        </div>        
    )
}

createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>    
);

