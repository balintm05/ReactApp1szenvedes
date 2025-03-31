import { useState, useEffect, useContext, useMemo } from 'react';
import { AuthContext } from '../context/AuthContext';
import { ThemeContext } from '../layout/Layout';
import AdminLayout from './AdminLayout';
import ThemeWrapper from '../layout/ThemeWrapper';
import { useNavigate } from 'react-router-dom';

function TeremListAdmin() {
    const { api } = useContext(AuthContext);
    const { darkMode } = useContext(ThemeContext);
    const [filter, setFilter] = useState({
        id: "",
        nev: ""
    });
    const [allTermek, setAllTermek] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const filteredTermek = useMemo(() => {
        return allTermek.filter(terem => {
            const teremId = terem?.id?.toString() || '';
            const teremNev = terem?.nev?.toLowerCase() || '';

            return (
                (filter.id === "" || teremId.includes(filter.id)) &&
                (filter.nev === "" || teremNev.includes(filter.nev.toLowerCase()))
            );
        });
    }, [allTermek, filter]);

    const handleFilterChange = (e) => {
        setFilter({ ...filter, [e.target.name]: e.target.value });
    };

    useEffect(() => {
        const controller = new AbortController();

        api.get('/Terem/get', {
            signal: controller.signal
        })
            .then(response => {
                const data = Array.isArray(response.data) ?
                    response.data.map(item => item.terem) :
                    [];

                setAllTermek(data);
                setLoading(false);
            })
            .catch((error) => {
                if (error.name !== 'CanceledError') {
                    setError(error.response?.data?.error || "Hiba a termek betöltésekor");
                    console.error("Error fetching terem data:", error);
                    setLoading(false);
                }
            });

        return () => controller.abort();
    }, [api]);

    if (loading) {
        return (
            <AdminLayout>
                <div className="admin-content-container">
                    <p>Betöltés...</p>
                </div>
            </AdminLayout>
        );
    }

    if (error) {
        return (
            <AdminLayout>
                <div className="admin-content-container">
                    <div className={`alert ${darkMode ? 'alert-danger' : 'alert-warning'}`}>
                        {error}
                    </div>
                </div>
            </AdminLayout>
        );
    }

    return (
        <AdminLayout>
            <ThemeWrapper className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 p-3 border-bottom">
                <h1 className="h2">Termek kezelése</h1>
                <button
                    className={`btn ${darkMode ? 'btn-success' : 'btn-outline-success'}`}
                    onClick={() => navigate('/admin/termek/add')}
                >
                    Új terem hozzáadása
                </button>
            </ThemeWrapper>

            <ThemeWrapper className="table-responsive">
                <table className={`table table-bordered ${darkMode ? 'table-dark' : ''}`}>
                    <thead>
                        <tr className={darkMode ? 'bg-dark text-light' : 'bg-light'}>
                            <th>ID</th>
                            <th>Név</th>
                            <th>Székek száma</th>
                            <th>Megjegyzés</th>
                            <th>Műveletek</th>
                        </tr>
                        <tr className={darkMode ? 'bg-secondary' : 'bg-light'}>
                            <th>
                                <input
                                    type="text"
                                    name="id"
                                    onChange={handleFilterChange}
                                    value={filter.id}
                                    className={`form-control ${darkMode ? 'bg-dark text-white border-light' : ''}`}
                                    placeholder="Keresés ID"
                                />
                            </th>
                            <th>
                                <input
                                    type="text"
                                    name="nev"
                                    onChange={handleFilterChange}
                                    value={filter.nev}
                                    className={`form-control ${darkMode ? 'bg-dark text-white border-light' : ''}`}
                                    placeholder="Keresés név"
                                />
                            </th>
                            <th></th>
                            <th></th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {filteredTermek.length > 0 ? (
                            filteredTermek.map(terem => {
                                const szekekCount = terem?.szekek?.length || 0;
                                return (
                                    <tr key={terem.id} className={darkMode ? 'table-dark' : ''}>
                                        <td>{terem.id}</td>
                                        <td>{terem.nev}</td>
                                        <td>{szekekCount}</td>
                                        <td>{terem.megjegyzes || '-'}</td>
                                        <td>
                                            <div className="d-flex gap-2">
                                                <button
                                                    className={`btn btn-sm ${darkMode ? 'btn-primary' : 'btn-outline-primary'}`}
                                                    onClick={() => navigate(`/admin/termek/edit/${terem.id}`)}
                                                >
                                                    Módosítás
                                                </button>
                                                <button
                                                    className={`btn btn-sm ${darkMode ? 'btn-danger' : 'btn-outline-danger'}`}
                                                    onClick={() => navigate(`/admin/termek/delete/${terem.id}`)}
                                                >
                                                    Törlés
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })
                        ) : (
                            <tr className={darkMode ? 'table-dark' : ''}>
                                <td colSpan={5} className="text-center py-4">
                                    {allTermek.length === 0 ? "Nincsenek termek az adatbázisban" : "Nincs találat a szűrési feltételek alapján"}
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </ThemeWrapper>
        </AdminLayout>
    );
}

export default TeremListAdmin;