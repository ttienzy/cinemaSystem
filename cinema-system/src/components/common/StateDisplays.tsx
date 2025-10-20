import React from 'react';
import { Ticket } from 'lucide-react';

export const LoadingSpinner: React.FC = () => (
    <div className="text-center py-10">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600 mx-auto"></div>
        <p className="mt-4 text-gray-600">Retrieving booking details...</p>
    </div>
);

export const InitialState: React.FC<{ title: string; message: string; }> = ({ title, message }) => (
    <div className="text-center py-10 border-2 border-dashed border-gray-300 rounded-lg bg-gray-50">
        <Ticket className="mx-auto h-12 w-12 text-gray-400" />
        <h3 className="mt-4 text-lg font-medium text-gray-800">{title}</h3>
        <p className="mt-1 text-sm text-gray-500">{message}</p>
    </div>
)