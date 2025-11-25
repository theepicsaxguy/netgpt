import React, { useState } from 'react';
import { usePostApiV1Conversations } from '../api/v1/generated/api';
import { useQueryClient } from '@tanstack/react-query';
import { X, Loader2, MessageSquarePlus, Terminal, ChevronRight } from 'lucide-react';
import { castTo } from '../types';

interface NewChatModalProps {
  onClose: () => void;
  onCreated: (id: string) => void;
}

const NewChatModal: React.FC<NewChatModalProps> = ({ onClose, onCreated }) => {
  const [title, setTitle] = useState('');
  const [systemPrompt, setSystemPrompt] = useState('');
  const queryClient = useQueryClient();

  const { mutate, isPending } = usePostApiV1Conversations({
    mutation: {
      onSuccess: (data: unknown) => {
        queryClient.invalidateQueries({ queryKey: ['/api/v1/v1/Conversations'] });
        const created = castTo<{id: string}>(data);
        if (created && created.id) {
            onCreated(created.id);
        } else {
            onClose(); 
        }
      },
      onError: () => {
        // Error handling visual feedback could be improved here
      }
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    mutate({
      data: {
        title: title || "New Session",
        configuration: systemPrompt ? {
            agents: [{
                instructions: systemPrompt,
                name: "Assistant",
                modelName: "gpt-4"
            }]
        } : undefined
      }
    });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/90 backdrop-blur-sm animate-in fade-in duration-300">
      <div className="bg-zinc-950 border border-zinc-800 rounded-xl w-full max-w-lg shadow-2xl shadow-black overflow-hidden animate-in zoom-in-95 duration-300 slide-in-from-bottom-2">
        <div className="flex items-center justify-between p-5 border-b border-zinc-800">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded bg-zinc-900 text-zinc-100 border border-zinc-800">
                <MessageSquarePlus size={18} />
            </div>
            <div>
              <h2 className="text-base font-bold text-zinc-100">Initialize Session</h2>
              <p className="text-xs text-zinc-500">Configure new agent parameters</p>
            </div>
          </div>
          <button onClick={onClose} className="text-zinc-500 hover:text-white transition-colors p-2 hover:bg-zinc-900 rounded">
            <X size={18} />
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          <div className="space-y-2">
            <label className="text-xs font-bold text-zinc-500 uppercase tracking-wider">Session Name</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. System Architecture Review"
              className="w-full bg-zinc-900 border border-zinc-800 rounded-lg px-4 py-3 text-zinc-100 focus:outline-none focus:border-zinc-600 transition-all placeholder-zinc-600 text-sm"
              autoFocus
            />
          </div>
          
          <div className="space-y-2">
            <div className="flex items-center gap-2">
                <Terminal size={12} className="text-zinc-400" />
                <label className="text-xs font-bold text-zinc-500 uppercase tracking-wider">System Instructions</label>
            </div>
            <textarea
              value={systemPrompt}
              onChange={(e) => setSystemPrompt(e.target.value)}
              placeholder="Define agent persona and constraints..."
              className="w-full bg-zinc-900 border border-zinc-800 rounded-lg px-4 py-3 text-zinc-100 focus:outline-none focus:border-zinc-600 transition-all placeholder-zinc-600 min-h-[120px] text-sm font-mono leading-relaxed"
            />
          </div>

          <div className="pt-2 flex gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2.5 bg-transparent hover:bg-zinc-900 text-zinc-400 hover:text-zinc-200 rounded-lg font-medium transition-colors text-sm"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isPending}
              className="flex-1 px-4 py-2.5 bg-zinc-100 hover:bg-white text-zinc-950 rounded-lg font-bold transition-all flex items-center justify-center gap-2 text-sm"
            >
              {isPending ? <Loader2 size={16} className="animate-spin" /> : (
                <>
                  <span>Create Session</span>
                  <ChevronRight size={16} />
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default NewChatModal;