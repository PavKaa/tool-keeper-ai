import React from 'react';
import styles from '../styles/components/Button.module.css';

export default function Button({ children, onClick }) {
  return (
    <button className={styles.button} onClick={onClick}>
      {children}
    </button>
  );
}