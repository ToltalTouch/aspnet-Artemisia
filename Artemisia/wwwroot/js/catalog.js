const CatalogViewer = () => {
    const [catalogHtml, setCatalogHtml] = React.useState('');
    const [isLoading, setIsLoading] = React.useState(false);
    const [error, setError] = React.useState(null);

    const loadCatalog = () => {
        setIsLoading(true);
        setError(null);
        fetch('/Home/Catalog') // URL para sua Action que retorna o HTML do catálogo
            .then(response => {
                if (!response.ok) {
                    throw new Error('Falha ao carregar o catálogo.');
                }
                return response.text(); // Pega o HTML como texto
            })
            .then(html => {
                setCatalogHtml(html);
                setIsLoading(false);
            })
            .catch(err => {
                setError(err.message);
                setIsLoading(false);
            });
    };

    return (
        <div>
            <button onClick={loadCatalog} className="btn btn-catalog">
                {isLoading ? 'Carregando...' : 'Ver Catálogo (com React)'}
            </button>
            {error && <p style={{ color: 'red' }}>{error}</p>}
            <div
                className="mt-4"
                dangerouslySetInnerHTML={{ __html: catalogHtml }}
            />
        </div>
    );
};

const domContainer = document.querySelector('#react-catalog-container');
const root = ReactDOM.createRoot(domContainer);
root.render(<CatalogViewer />);