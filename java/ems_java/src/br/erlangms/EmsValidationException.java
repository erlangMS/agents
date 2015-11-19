package br.erlangms;

import java.util.ArrayList;
import java.util.List;

import javax.ejb.ApplicationException;

@ApplicationException(rollback = false)
public class EmsValidationException extends RuntimeException {
	private static final long serialVersionUID = -8316509235178192483L;
	private List<String> errors;
	
	public EmsValidationException () {
		super();
		errors = new ArrayList<>();
	}
	
	public EmsValidationException (String e) {
		super(e);
		errors = new ArrayList<>();
		errors.add(e);
	}
	
	
	public EmsValidationException (List <String> l) {
		errors = l;
	}
	
	public void addError(String error) {
		errors.add(error);
	}
	
	public List<String> getErrors () {
		return errors;
	}
	
}
