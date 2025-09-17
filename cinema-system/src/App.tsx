import { BrowserRouter } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store/store';
import { AppRoutes } from './routes/AppRoutes';
import './styles/App.css';
import { SignalRProvider } from './contexts/SignalRContext';

const App = () => {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <SignalRProvider >
          <AppRoutes />
        </SignalRProvider>
      </BrowserRouter>
    </Provider>
  );
}

export default App;