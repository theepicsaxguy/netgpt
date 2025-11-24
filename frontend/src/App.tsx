import React, { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import SideBar from './components/SideBar';
import ChatArea from './components/ChatArea';
import ChatModal from './components/ChatModal';

const queryClient = new QueryClient();

export default function App(): JSX.Element {
  const [selectedConversationId, setSelectedConversationId] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <QueryClientProvider client={queryClient}>
      <div className="flex h-screen bg-zinc-950 text-zinc-100 font-sans antialiased">
        <SideBar 
          selectedId={selectedConversationId} 
          onSelect={setSelectedConversationId}
          onNewChat={() => setIsModalOpen(true)}
        />
        <main className="flex-1 flex flex-col relative overflow-hidden bg-zinc-950">
          {selectedConversationId ? (
            <ChatArea conversationId={selectedConversationId} />
          ) : (
            <div className="flex flex-col items-center justify-center h-full text-zinc-500 p-8 text-center animate-in fade-in duration-700">
              <div className="w-20 h-20 rounded-2xl bg-zinc-900 border border-zinc-800 flex items-center justify-center mb-8 shadow-2xl shadow-black/50">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" className="text-zinc-100">
                  <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                </svg>
              </div>
              <h1 className="text-3xl font-bold mb-3 text-zinc-100 tracking-tight">NetGPT</h1>
              <p className="max-w-md text-zinc-400 mb-8 leading-relaxed">
                Your advanced AI assistant powered by .NET. Select a conversation to begin or start a new session.
              </p>
              <button 
                onClick={() => setIsModalOpen(true)}
                className="group relative px-6 py-3 bg-zinc-100 hover:bg-white text-zinc-950 rounded-lg font-medium transition-all duration-200 shadow-[0_0_20px_rgba(255,255,255,0.1)] hover:shadow-[0_0_25px_rgba(255,255,255,0.2)]"
              >
                Start New Session
              </button>
            </div>
          )}
        </main>
        
        {isModalOpen && (
          <ChatModal 
            onClose={() => setIsModalOpen(false)} 
            onCreated={(id: string) => {
              setSelectedConversationId(id);
              setIsModalOpen(false);
            }} 
          />
        )}
      </div>
    </QueryClientProvider>
  );
}